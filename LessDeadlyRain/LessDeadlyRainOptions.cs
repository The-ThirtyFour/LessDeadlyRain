using Menu.Remix.MixedUI;
using System;
using UnityEngine;

namespace LessDeadlyRain;

public class LessDeadlyRainOptions : OptionInterface
{
	public readonly Configurable<bool> lessDeadlyRain;

	public readonly Configurable<bool> noRainFlooding;

	public readonly Configurable<bool> arenaMode;

	public readonly Configurable<float> buildupMultiplier;

	public readonly Configurable<int> screenShakeIntensity;

	public LessDeadlyRainOptions()
	{
		lessDeadlyRain = config.Bind("fcldr_lessDeadlyRain", defaultValue: true);
		noRainFlooding = config.Bind("fcldr_noRainFlooding", defaultValue: false);
		arenaMode = config.Bind("fcldr_arenaMode", defaultValue: false);
		buildupMultiplier = config.Bind("fcldr_buildupMultiplier", 1f);
		screenShakeIntensity = config.Bind("fcldr_screenShakeIntensity", 25);

	}

	public override void Initialize()
	{
		OpTab opTab = new OpTab(this, "Options");
		Tabs = new OpTab[1] { opTab };
		OpContainer opContainer = new OpContainer(new Vector2(0f, 0f));
		opTab.AddItems(opContainer);
		UIelement[] elements = new UIelement[11]
		{
			new OpLabel(25f, 550f, "Options", bigText: true),
			new OpCheckBox(lessDeadlyRain, 25f, 520f)
			{
				description = "At the end of the cycle, prevent the entire screen from being filled with instant kill rain."
			},
			new OpLabel(55f, 523f, "Disable entire-screen death rain"),
			new OpCheckBox(noRainFlooding, 25f, 490f)
			{
				description = "At the end of the cycle, prevent interior rooms from being flooded with water."
			},
			new OpLabel(55f, 493f, "Disable flooding"),
			new OpCheckBox(arenaMode, 25f, 460f)
			{
				description = "Also apply these settings in Arena modes."
			},
			new OpLabel(55f, 463f, "Apply in Arena modes"),
			new OpLabel(25f, 433f, "Rain buildup timer:"),
			new OpUpdownCustom(buildupMultiplier, new Vector2(145f, 427f), 70f, 0.1f, 100f, 1)
			{
				description = "Change the amount of time it takes for rain to become deadly. (For example, 2.0 will make it take twice as long)"
			},
			new OpLabel(25f, 403f, "Screenshake intensity:"),
			new OpSlider(screenShakeIntensity, new Vector2(25f, 370f), 150)
			{
				max = 100,
				description = "Change the intensity of screenshake during end-of-cycle rain. 0% is no screenshake, 100% is full screenshake."
			}
		};
		opTab.AddItems(elements);
	}
}

public class OpUpdownCustom : OpUpdown
{
	
    public OpUpdownCustom(Configurable<int> config, Vector2 pos, float sizeX) : base(config, pos, sizeX)
    {
    }

    public OpUpdownCustom(Configurable<float> config, Vector2 pos, float sizeX, byte decimalNum = 1) : base(config, pos, sizeX, decimalNum)
    {
    }

    public OpUpdownCustom(Configurable<float> config, Vector2 pos, float sizeX, float _fMin, float _fMax, byte decimalNum = 1) : base(config, pos, sizeX, decimalNum)
    {
		this._fMin = _fMin;
		this._fMax = _fMax;
    }
}