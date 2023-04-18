using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using System.Windows.Forms;

namespace SkillGems
{
    public class SkillGemsSettings : ISettings
    {
        //Mandatory setting to allow enabling/disabling your plugin
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
        public ToggleNode ReturnMouseToStart { get; set; } = new ToggleNode(true);
        public ToggleNode AddPingIntoDelay { get; set; } = new ToggleNode(false);
        public HotkeyNode Run { get; set; } = new HotkeyNode(Keys.A);
        public RangeNode<int> DelayBetweenEachGemClick { get; set; } = new RangeNode<int>(20, 0, 1000);
        public RangeNode<int> DelayBetweenEachMouseEvent { get; set; } = new RangeNode<int>(20, 0, 1000);

        //Put all your settings here if you can.
        //There's a bunch of ready-made setting nodes,
        //nested menu support and even custom callbacks are supported.
        //If you want to override DrawSettings instead, you better have a very good reason.
    }
}