using System;
using System.ComponentModel.Design;
using BepInEx;
using IL;
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

	private float timeInThisMode;

	private int pulsing = -1;

	private bool ndr = false;

	private float maxIntensity = 0.45f;

	public void OnEnable()
	{
		On.RainWorld.OnModsInit += OnModsInitHook;
		On.GlobalRain.DeathRain.DeathRainUpdate += DeathRainUpdateHook;
		On.GlobalRain.DeathRain.NextDeathRainMode += NextDeathRainModeHook;
		On.RainCycle.Update += RainCycleUpdateHook;
        On.GlobalRain.Update += GlobalRain_Update;
		On.GlobalRain.InitDeathRain += InitDeathRainHook;
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
        if (options.noDeathRain.Value)
        {
            ndr = true;
            return;
        }
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

            self.globalRain.bulletTimer = 0;
			if (pulsing == 0)
			{
				self.globalRain.bulletRainDensity = Mathf.Lerp(0.5f, 0.7f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
				self.globalRain.flood -= globalRain.floodSpeed;
                self.globalRain.Intensity = 0f;
                self.globalRain.RumbleSound = -1f;
                self.globalRain.MicroScreenShake = 0f;
				self.globalRain.ScreenShake = 0f;

			}
			if (pulsing == 1)
			{
                self.globalRain.Intensity = Mathf.Lerp(0f, 0.6f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
				self.globalRain.bulletRainDensity = Mathf.Lerp(0.7f, 0.4f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));

			}
			if (pulsing == 2)
			{
				if (self.globalRain.Intensity > 0.6)
				{
                    self.globalRain.Intensity = 0.6f;
				}
			}
			if (pulsing == 3)
			{
                self.globalRain.Intensity = Mathf.Lerp(0.6f, 0f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
				self.globalRain.bulletRainDensity = Mathf.Lerp(0.3f, 0.5f, (float)self.GetType().GetField("progression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
			}
		}
        if (options.noRainFlooding.Value)
		{
            self.globalRain.flood -= self.globalRain.floodSpeed;
		}
		float num = (float)options.screenShakeIntensity.Value * 0.01f;
        self.globalRain.MicroScreenShake *= pulsing == 0 ? num : 0;
        self.globalRain.ScreenShake *= pulsing == 0 ? num : 0;
		
	}

	private void NextDeathRainModeHook(On.GlobalRain.DeathRain.orig_NextDeathRainMode orig, GlobalRain.DeathRain self)
	{
		if (ndr)
		{
			return;
		}
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

	private void InitDeathRainHook(On.GlobalRain.orig_InitDeathRain orig, GlobalRain self)
	{
		if (options.noDeathRain.Value)
		{
			ndr = true;
		}
		if (ndr)
		{
			return;
		}
		orig(self);
	}
    private void GlobalRain_Update(On.GlobalRain.orig_Update orig, GlobalRain self)
    {
		orig(self);

		oldRumbleSound = self.RumbleSound;
		oldScreenShake = self.ScreenShake;
		oldMicroScreenShake = self.MicroScreenShake;

		if (!ndr)
		{
			return;
		}

		if (options.noDeathRain.Value)
		{
			if (self.deathRain != null)
			{
				self.deathRain = null;
			}

			if (self.flood < 0)
			{
				self.flood = 0;
			}

			if (options.pulsingRain.Value)
			{
				if (pulseTimer <= 0)
				{
					if (pulsing == -1) //activate
					{
						pulsing = 0;
					}
                    if (pulsing == 3) //no rain
					{
						pulseTimer = 1000;
                        pulsing = 0;
						self.RumbleSound = 0f;
					}
                    else if (pulsing == 0) //rain transition
					{
						pulseTimer = 100;
                        pulsing = 1;
					}
                    else if (pulsing == 1) //rain
					{
						pulseTimer = 1000;
                        pulsing = 2;
						self.RumbleSound = oldRumbleSound;
					}
                    else if (pulsing == 2) //no rain transition
					{
						pulseTimer = 100;
                        pulsing = 3;
					}
					timeInThisMode = pulseTimer;

                    foreach (Room room in self.game.world.activeRooms)
                    {
                        room.roomRain.rumbleSound.Volume = self.RumbleSound * room.roomSettings.RumbleIntensity;
						room.roomRain.rumbleSound.Update();
                    }
                }

				pulseTimer -= 1;

				//update
				if (pulsing == 0) // no rain
				{
					self.Intensity = 0f;
					self.bulletRainDensity = Mathf.Lerp(0.35f, 0.2f, pulseTimer / timeInThisMode);
					self.flood -= self.floodSpeed;
					self.ScreenShake = 0f;
					self.MicroScreenShake = 0f;
					self.bulletTimer = 0;
				}
				if (pulsing == 1) //rain transition
				{
                    self.bulletRainDensity = Mathf.Lerp(0.7f, 0.4f, pulseTimer / timeInThisMode);
                    self.Intensity = Mathf.Lerp(maxIntensity, 0f, pulseTimer / timeInThisMode); //lerp fucking reversed...
				}
				if (pulsing == 2) //rain
				{
					self.Intensity = maxIntensity;
                    self.flood += self.floodSpeed;
					//self.ScreenShake = oldScreenShake;
					//self.MicroScreenShake = oldMicroScreenShake;
					self.ScreenShake = 1;
					self.MicroScreenShake = 1;
                }
                if (pulsing == 3) //no rain transition
				{
                    self.bulletRainDensity = Mathf.Lerp(0.3f, 0.7f, pulseTimer / timeInThisMode);
                    self.Intensity = Mathf.Lerp(0f, maxIntensity, pulseTimer / timeInThisMode); //lerp fucking reversed...
                }
            }
		}
    }
}
