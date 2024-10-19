using System;
using System.ComponentModel.Design;
using BepInEx;
using On;
using UnityEngine;

namespace LessDeadlyRain;

[BepInPlugin("floofcheeks.lessdeadlyrain", "Less Deadly Rain", "1.2.2")]
[BepInDependency("com.dual.catnap", BepInDependency.DependencyFlags.SoftDependency)]
public class LessDeadlyRain : BaseUnityPlugin
{
	private LessDeadlyRainOptions options;

	private float oldMicroScreenShake;

	private float oldScreenShake;

	private float oldRumbleSound;

	private float pulseTimer;

	private int pulsing = -1;


	public void OnEnable()
	{
		On.RainWorld.OnModsInit += OnModsInitHook;
		On.GlobalRain.DeathRain.DeathRainUpdate += DeathRainUpdateHook;
		On.GlobalRain.DeathRain.NextDeathRainMode += NextDeathRainModeHook;
		On.RainCycle.Update += RainCycleUpdateHook;
	}

	private void OnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
	{
		orig(self);
		try
		{
			options = new LessDeadlyRainOptions();
			MachineConnector.SetRegisteredOI("floofcheeks.lessdeadlyrain", options);
		}
		catch (Exception arg)
		{
			base.Logger.LogError($"Error creating options: {arg}");
		}
	}

	private void DeathRainUpdateHook(On.GlobalRain.DeathRain.orig_DeathRainUpdate orig, GlobalRain.DeathRain self)
	{
		GlobalRain globalRain = self.globalRain;
		RainWorldGame game = globalRain.game;
		if (game.IsStorySession || (game.IsArenaSession && options.arenaMode.Value))
		{
			globalRain.MicroScreenShake = oldMicroScreenShake;
			globalRain.ScreenShake = oldScreenShake;
			globalRain.RumbleSound = oldRumbleSound;
		}
		orig(self);
		if (!game.IsStorySession && (!game.IsArenaSession || !options.arenaMode.Value))
		{
			return;
		}
		oldMicroScreenShake = globalRain.MicroScreenShake;
		oldScreenShake = globalRain.ScreenShake;
		oldRumbleSound = globalRain.RumbleSound;
		if (options.lessDeadlyRain.Value)
		{
			if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.FinalBuildUp || self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.Mayhem)
			{
				base.Logger.LogDebug("Reached lethal rain mode. Jumping back to GradeAPlateu mode");
				self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.GradeAPlateu;

				if (options.pulsingRain.Value)
				{
					pulsing = 2;
					self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.None;
					self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, Mathf.Lerp(750f, 800f, UnityEngine.Random.value));
					self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 0f);
				}

			}
			if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeAPlateu && !options.pulsingRain.Value)
			{
				//self.timeInThisMode = 10000f;
				//self.progression = 0f;

				self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 10000f);
				self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 0f);
			}
			if (globalRain.Intensity > 0.6f)
			{
				globalRain.Intensity = 0.6f;
			}
			globalRain.RumbleSound *= 0.5f;

			pulseTimer = Mathf.Lerp(2000, 0, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));

			if (pulseTimer <= 10)
			{
				//if (self.deathRainMode == GlobalRain.DeathRain.DeathRainMode.GradeAPlateu)
				if (pulsing == 3)
				{
					self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.None;

					self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, Mathf.Lerp(650f, 700f, UnityEngine.Random.value));
					self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 0f);

					globalRain.Intensity = 0f;
					globalRain.bulletRainDensity = 0.5f;

					pulsing = 0;
				}
				else if (pulsing == 0)
				{
					self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.None;

					self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 200f);
					self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 0f);

					pulsing = 1;
				}
				else if (pulsing == 1)
				{
					self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.None;

					self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, Mathf.Lerp(650f, 700f, UnityEngine.Random.value));
					self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 0f);

					globalRain.Intensity = 0.6f;

					pulsing = 2;
				}
				else if (pulsing == 2)
				{
					self.deathRainMode = GlobalRain.DeathRain.DeathRainMode.None;

					self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 200f);
					self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 0f);

					pulsing = 3;
				}


				pulseTimer = 2000f;
			}

			globalRain.bulletTimer = 0;
			if (pulsing == 0)
			{
				self.globalRain.bulletRainDensity = Mathf.Lerp(0.5f, 0.8f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
				globalRain.flood -= globalRain.floodSpeed;
				globalRain.Intensity = 0f;
				globalRain.RumbleSound = -1f;
				globalRain.MicroScreenShake = 0f;
				globalRain.ScreenShake = 0f;

			}
			if (pulsing == 1)
			{
				globalRain.Intensity = Mathf.Lerp(0f, 0.6f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
				self.globalRain.bulletRainDensity = Mathf.Lerp(0.8f, 0.6f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));

			}
			if (pulsing == 2)
			{
				if (globalRain.Intensity > 0.6)
				{
					globalRain.Intensity = 0.6f;
				}
			}
			if (pulsing == 3)
			{
				globalRain.Intensity = Mathf.Lerp(0.6f, 0f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
				self.globalRain.bulletRainDensity = Mathf.Lerp(0.3f, 0.5f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
			}
		}
        if (options.noRainFlooding.Value)
		{
			globalRain.flood -= globalRain.floodSpeed;
		}
		float num = (float)options.screenShakeIntensity.Value * 0.01f;
		globalRain.MicroScreenShake *= pulsing == 0 ? num : 0;
		globalRain.ScreenShake *= pulsing == 0 ? num : 0;
		
	}

	private void NextDeathRainModeHook(On.GlobalRain.DeathRain.orig_NextDeathRainMode orig, GlobalRain.DeathRain self)
	{
		orig(self);
		if ((self.globalRain.game.IsStorySession || (self.globalRain.game.IsArenaSession && options.arenaMode.Value)) && options.buildupMultiplier.Value != 1f)
		{
			//base.Logger.LogDebug($"Rain mode progressed. Multiplying time in this mode by {options.buildupMultiplier.Value} ({self.timeInThisMode} -> {self.timeInThisMode * options.buildupMultiplier.Value})");
			base.Logger.LogDebug($"Rain mode progressed. Multiplying time in this mode by {options.buildupMultiplier.Value} ({self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self)} -> {(float)self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self) * options.buildupMultiplier.Value})");
			//self.timeInThisMode *= options.buildupMultiplier.Value;
            self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self,
            (float)self.GetType().GetField("timeInThisMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self) * options.buildupMultiplier.Value);
        }
    }

	private void RainCycleUpdateHook(On.RainCycle.orig_Update orig, RainCycle self)
	{
		orig(self);
		if (self.world.game.IsArenaSession && options.arenaMode.Value && self.timer > self.cycleLength + 900)
		{
			self.timer = self.cycleLength + 900;
		}
	}

	
}
