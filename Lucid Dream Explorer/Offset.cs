using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucid_Dream_Explorer
{
    abstract class Offset
    {
        public static readonly int X = 0x91e74;
        public static readonly int Y = 0x91e7c;
        public static readonly int Z = 0x91e78;

        public static readonly int map = 0x8abf8;
        public static readonly int map_pos = 0x9169c;

        public static readonly int chart_draw_x = 0x94008;
        public static readonly int chart_draw_y = 0x9400c;
        public static readonly int chart_return_x = 0x94014;
        public static readonly int chart_return_y = 0x94016;

        public static readonly int day = 0x916B0;
        public static readonly int total_flashback_score = 0x916B4;

        public static readonly int dream_timer = 0x91554;
        public static readonly int dream_time_limit = 0x91664;

        public static readonly int last_event_x = 0x91674;
        public static readonly int last_event_y = 0x91675;
        public static readonly int total_event_x = 0x91678;
        public static readonly int total_event_y = 0x9167c;
        public static readonly int events = 0x91680;

        public static readonly int last_obj_event_x = 0x91684;
        public static readonly int last_obj_event_y = 0x91685;
        public static readonly int total_obj_event_x = 0x91688;
        public static readonly int total_obj_event_y = 0x9168c;
        public static readonly int obj_events = 0x91690;

        public static readonly uint gray_ptr = 0x88D2C;
        public static readonly int gray_flag_enabled = 0x8C;

        /*For future reference: 0x91fc8 contains the ram value for the currently selected option in the main menu
            0: START
            1: FLASHBACK (Even if it's not unlocked)
            2: SAVE
            3: LOAD
            4: GRAPH
            5: SHAKE
        Search for these values in Cheat Engine then substract 0x91fc8 to get the current start of memory.
        Search for THAT address until you get something with a "'.exe' + something" address.*/

        public static readonly int ePSXeMemstart = 0x14D2020;
        public static readonly int ePSXeVersion = 0x1551B5C;

        public static readonly int psxfinMemstart = 0x171A5C;
        public static readonly int psxfinVersion = 0x128D34;

        public static readonly int xebraMemstart = 0x54920;
        public static readonly int xebraVersion = 0x0; //TODO

        public static readonly int nocashMemstart = 0xC74C8;
        public static readonly int nocashMemstartLite = 0x80E90;
        public static readonly int nocashVersion = 0x11c5;
        public static readonly int nocashEdition = 0xC3DA5; //Garbled string (-85) don't worry about it.
    }
}
