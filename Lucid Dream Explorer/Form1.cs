using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lucid_Dream_Explorer
{
    public partial class Form1 : Form
    {

        private class EventRatingContainer
        {
            public int sum_x, sum_y, last_x, last_y, amount = 0;
            public int avg_x, avg_y = 0;
            public float raw_avg_x, raw_avg_y = 0;
            private static float CalcRawAvg(int sum, int amt)
            {
                return ((float)sum) / amt;
            }
            private static int CalcFinalValue(int avg, int bonus)
            {
                int value = avg + bonus / 3;

                if (value >= 10)
                {
                    value = -9;
                }
                else if (value < -9)
                {
                    value = 9;
                }
                return value;
            }
            public void UpdateAverages()
            {
                if (amount <= 0)
                {
                    avg_x = 0; avg_y = 0;
                    raw_avg_x = 0; raw_avg_y = 0;
                }
                else
                {
                    raw_avg_x = CalcRawAvg(sum_x, amount);
                    avg_x = CalcFinalValue((int)raw_avg_x, last_x);
                    raw_avg_y = CalcRawAvg(sum_y, amount);
                    avg_y = CalcFinalValue((int)raw_avg_y, last_y);
                }
            }
        }
        private EventRatingContainer areaEvents, entityEvents;

        private static int[] special_days = { 2, 7, 14, 15, 21, 28, 35, 42, 43, 49, 56, 63, 70, 77, 81, 84, 91, 98, 105, 112, 119, 120, 126, 134, 141, 148, 155, 162, 169, 176, 183, 190, 197, 202, 204, 259, 267, 284, 308, 328, 358, 367 };
        private int our_current_day = -1;
        private static string[] maps = { "Bright Moon Cottage", "Pit & Temple", "Kyoto", "The Natural World", "Happy Town", "Violence District", "Moonlight Tower", "Temple Dojo", "Flesh Tunnels", "Clockwork Machines", "Long Hallway", "Sun Faces Heave", "Black Space", "Monument Park" };
        private static string[] sounds = { "00 - [Jangling]", "01 - [Opera Singer?]", "02 - [Jiggling]", "03 - Grass, Sand, H.T. Footstep", "04 - Train", "05 - Astronaut, Trumpeters", "06 - H.T. Squeak Footstep, Pterodactyl, Kissing Lips", "07 - Flowing Water (Natural World)", "08 - Link", "09 - H.T. Wood Footstep", "10 - Deep Rumbling", "11 - V.D. Dock Footstep", "12 - Wooden Bridge Footstep", "13 - [Tengu?]", "14 - Drumming (Kyoto)", "15 - H.T. Wet Footstep", "16 - [Tengu?]", "17 - [Clockwork Machine?]", "18 - Snarl (Lions, Dojo Dog)", "19 - Wings Flapping (Pterodactyl)", "20 - [Unknown]", "21 - Fetus Noise, Rocket Ship", "22 - Breathing (Sleeper in BMC)", "23 - BMC Birds Tweeting", "24 - [Long Hallway End?]", "25 - Horses Galloping (Natural World)", "26 - [UFO?]", "27 - Kyoto Grass Footstep", "28 - Generic Footstep (Tunnels, V.D., BMC Tile, NW Rock)", "29 - Water Footstep (NW River, Flesh Tunnel Slosh)" };

        private MemoryIO mio;
        private bool busy, blinkMarker;

        public Form1()
        {
            InitializeComponent();

            timer1.Start();

            areaEvents = new EventRatingContainer();
            entityEvents = new EventRatingContainer();
        }

        private IntPtr? ReadDEbaseAdress()
        {
            IntPtr? addr = Emulator.baseAddr;

            /**
             * Failed with all emulators
             * Update labels and buttons
             **/
            if (addr == null)
            {
                //Hook failed!
            }
            else
            {
                if (Emulator.emulator == Emulator.PSXFIN) //psxfin: Update labels and buttons
                {
                    //aa
                }
                if (Emulator.emulator == Emulator.EPSXE) //ePSXe: Update labels and buttons
                {
                    //a
                }
            }
            return addr;
        }

        private int? ReadVal(int offset)
        {
            IntPtr? baseAddr = ReadDEbaseAdress();
            byte[] readBuf = new byte[4];

            MemoryIO mio = new MemoryIO(Emulator.emulator);
            if (!mio.processOK()) return null;

            IntPtr mappointer = new IntPtr((int)baseAddr + offset);
            mio.MemoryRead(mappointer, readBuf);
            int map = BitConverter.ToInt32(readBuf, 0);
            if (Range.isInsideRange(Range.map, map)) return map;
            return null;
        }

        private int? UpdateReadValue(int offset)
        {
            if (mio == null)
            {
                mio = new MemoryIO(Emulator.emulator);
            }
            IntPtr? addr = Emulator.baseAddr;
            if (addr == null) return null;
            IntPtr pointer = new IntPtr((int)addr + offset);
            byte[] readBuf = new byte[4];
            mio.MemoryRead(pointer, readBuf);
            int value = BitConverter.ToInt32(readBuf, 0);
            return value;
        }

        private int? GetIngamePointer(int offset)
        {
            if (mio == null)
            {
                mio = new MemoryIO(Emulator.emulator);
            }
            IntPtr? addr = Emulator.baseAddr;
            if (addr == null) return null;
            IntPtr pointer = new IntPtr((int)addr + offset);
            byte[] readBuf = new byte[4];
            mio.MemoryRead(pointer, readBuf);
            uint val = BitConverter.ToUInt32(readBuf, 0);
            val &= ~0xA0000000; //Unset virtual memory cached/uncached flags
            if (val == 0) return null;
            return (int)val;
        }

        private int? UpdateReadByte(int offset)
        {
            if (mio == null)
            {
                mio = new MemoryIO(Emulator.emulator);
            }
            IntPtr? addr = Emulator.baseAddr;
            if (addr == null) return null;
            IntPtr pointer = new IntPtr((int)addr + offset);
            byte[] readBuf = new byte[1];
            mio.MemoryRead(pointer, readBuf);
            int value = (sbyte)readBuf[0];
            return value;
        }

        private void ButtonSetByte(TextBox textBox, int offset)
        {

            IntPtr? baseAddr = ReadDEbaseAdress();
            if (baseAddr == null)
            {
                return; //Fatal error
            }

            if (textBox.Text == "")
            {
                return;
            }
            byte[] writeBuf = new byte[1];
            writeBuf[0] = (byte)sbyte.Parse(textBox.Text);

            MemoryIO mio = new MemoryIO(Emulator.emulator);
            if (!mio.processOK()) return;

            /**
             * Write position variable
             **/
            IntPtr pointer = new IntPtr((int)baseAddr + offset);
            mio.MemoryWrite(pointer, writeBuf);
        }

        private void DrawDreamChart(float x, float y, bool blink, EventRatingContainer areas, EventRatingContainer objs)
        {
            Pen ChunkPen = new Pen(Brushes.DodgerBlue, 3.0f);
            Pen smallOutline = new Pen(Brushes.DodgerBlue, 1.0f);
            Pen smallDashed = new Pen(Brushes.DodgerBlue, 2.0f);
            smallDashed.DashStyle = DashStyle.Dot;
            smallDashed.EndCap = LineCap.ArrowAnchor;
            int center = pictureBox1.Width / 2; //Width == height here
            center -= 8; //Square center

            Bitmap baseImg = Lucid_Dream_Explorer.Properties.Resources.DreamChart;
            Graphics g = Graphics.FromImage(baseImg);
            if (blink)
            {
                g.FillRectangle(Brushes.Red, center + ((int)x) * 16, center - ((int)y) * 16, 16, 16);
            }
            /*else
            {
                g.FillRectangle(System.Drawing.Brushes.DarkRed, center + x * 16, center - y * 16, 16, 16);
            }*/
            g.DrawRectangle(ChunkPen, center + areas.last_x * 16 + 1, center - areas.last_y * 16 + 1, 14, 14);

            center += 8;

            float rawX = center + areas.raw_avg_x * 16;
            float rawY = center - areas.raw_avg_y * 16;
            float lankedX = rawX + areas.last_x / 3 * 16;
            float lankedY = rawY - areas.last_y / 3 * 16;

            g.DrawLine(smallDashed, rawX, rawY, lankedX, lankedY);
            if (areas.amount > 1)
            {
                g.DrawEllipse(smallOutline, rawX - 4, rawY - 4, 8, 8);
            }
            
            if (objs.amount > 0)
            {

                rawX = center + objs.raw_avg_x * 16;
                rawY = center - objs.raw_avg_y * 16;
                lankedX = rawX + objs.last_x / 3 * 16;
                lankedY = rawY - objs.last_y / 3 * 16;

                smallDashed.Brush = Brushes.Green;
                g.DrawLine(smallDashed, rawX, rawY, lankedX, lankedY);
                smallOutline.Brush = Brushes.Green;
                g.DrawEllipse(smallOutline, rawX - 4, rawY - 4, 8, 8);

                int objX = center + objs.avg_x * 16;
                int objY = center - objs.avg_y * 16;
                int areaX = center + areas.avg_x * 16;
                int areaY = center - areas.avg_y * 16;
                float finalX = center + x * 16;
                float finalY = center - y * 16;

                smallDashed.Brush = Brushes.Black;
                smallDashed.EndCap = LineCap.NoAnchor;
                g.DrawLine(smallDashed, areaX, areaY, objX, objY);
                smallOutline.Brush = Brushes.Black;
                g.FillEllipse(Brushes.DodgerBlue, areaX - 4, areaY - 4, 8, 8);
                g.DrawEllipse(smallOutline, areaX - 4, areaY - 4, 8, 8);
                g.FillEllipse(Brushes.Green, objX - 4, objY - 4, 8, 8);
                g.DrawEllipse(smallOutline, objX - 4, objY - 4, 8, 8);
                g.FillEllipse(Brushes.Red, finalX - 4, finalY - 4, 8, 8);
                g.DrawEllipse(smallOutline, finalX - 4, finalY - 4, 8, 8);
            }

            pictureBox1.Image = baseImg;
            ChunkPen.Dispose();
            smallOutline.Dispose();
            smallDashed.Dispose();
            g.Dispose();

        }

        //TODO: Use sender or e to get nud and offset
        private void ButtonSet(TextBox textBox, int offset)
        {

            IntPtr? baseAddr = ReadDEbaseAdress();
            if (baseAddr == null)
            {
                return; //Fatal error
            }

            if (textBox.Text == "")
            {
                return;
            }
            byte[] writeBuf = new byte[4];
            writeBuf = BitConverter.GetBytes(Convert.ToInt32(textBox.Text));

            MemoryIO mio = new MemoryIO(Emulator.emulator);
            if (!mio.processOK()) return;

            /**
             * Write position variable
             **/
            IntPtr pointer = new IntPtr((int)baseAddr + offset);
            mio.MemoryWrite(pointer, writeBuf);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!busy)
            {
                busy = true;
                updateStuff();
                busy = false;
            }
        }

        private string daysToNextSpecial(int currentDay)
        {
            foreach (int spday in special_days)
            {
                //go through the special day list until you find one that's equal or greater 
                if(currentDay == spday)
                {
                    return "Special day";
                }
                else if (currentDay + 1 == spday)
                {
                    return "Special day Tomorrow!";
                }
                else if (currentDay + 1 < spday)
                {
                    return String.Format("Next special in {0} days", spday - currentDay);
                }
            }
            return "invalid?";
        }

        private void updateStuff()
        {
            
            if (ReadDEbaseAdress() == null)
            {
                return;
            }
            //readmemandupdatelabel1
            if (ReadVal(Offset.map).HasValue)
            {
                int k = ReadVal(Offset.map).Value;

                //Map
                if (!textBox1.Focused && k >= 0 && k < 14)
                {
                    textBox1.Text = maps[k];
                }
                if (!textBox2.Focused)
                {
                    textBox2.Text = k.ToString();
                }
            }

            //random
            if (!textBox35.Focused)
            {
                textBox35.Text = UpdateReadValue(Offset.RNG).Value.ToString();
            }

            //Day
            int currentDay = ReadVal(Offset.day).Value;
            if (!textBox3.Focused)
            {
                textBox3.Text = currentDay.ToString();
                if (currentDay != our_current_day)
                {
                    label28.Text = daysToNextSpecial(currentDay + 1);
                    our_current_day = currentDay;
                }
            }

            //Previous Dream
            int previousDay = currentDay - 1;
            if (currentDay <= 0)
            {
                previousDay = 364;
            }

            int previousDayMood_x, previousDayMood_y;
            if (!textBox24.Focused)
            {
                previousDayMood_x = UpdateReadByte(Offset.rating_charts + previousDay * 2).Value;
                textBox24.Text = previousDayMood_x.ToString();
            }
            if (!textBox25.Focused)
            {
                previousDayMood_y = UpdateReadByte(Offset.rating_charts + 1 + previousDay * 2).Value;
                textBox25.Text = previousDayMood_y.ToString();
            }

            //Flashback score
            if (!textBox30.Focused)
            {
                int flashbackProgress = UpdateReadValue(Offset.total_flashback_score).Value;
                textBox30.Text = flashbackProgress.ToString();
                label24.Text = (flashbackProgress/100000).ToString()+"%";
            }

            //Timers
            textBox4.Text = UpdateReadValue(0x8AC70).Value.ToString();
            if (!textBox31.Focused)
            {
                textBox31.Text = UpdateReadValue(Offset.dream_timer).Value.ToString();
            }
            if (!textBox32.Focused)
            {
                textBox32.Text = UpdateReadValue(Offset.dream_time_limit).Value.ToString();
            }

            //Events
            areaEvents.amount = UpdateReadValue(Offset.events).Value;
            if (!textBox8.Focused) textBox8.Text = areaEvents.amount.ToString();
            areaEvents.last_x = UpdateReadByte(Offset.last_event_x).Value;
            if (!textBox9.Focused) textBox9.Text = areaEvents.last_x.ToString();
            areaEvents.last_y = UpdateReadByte(Offset.last_event_y).Value;
            if (!textBox12.Focused) textBox12.Text = areaEvents.last_y.ToString();
            areaEvents.sum_x = UpdateReadValue(Offset.total_event_x).Value;
            if (!textBox10.Focused) textBox10.Text = areaEvents.sum_x.ToString();
            areaEvents.sum_y = UpdateReadValue(Offset.total_event_y).Value;
            if (!textBox11.Focused) textBox11.Text = areaEvents.sum_y.ToString();

            areaEvents.UpdateAverages();
            textBox13.Text = areaEvents.avg_x.ToString();
            textBox14.Text = areaEvents.avg_y.ToString();

            entityEvents.amount = UpdateReadValue(Offset.obj_events).Value;
            if (!textBox21.Focused) textBox21.Text = entityEvents.amount.ToString();
            entityEvents.last_x = UpdateReadByte(Offset.last_obj_event_x).Value;
            if (!textBox20.Focused) textBox20.Text = entityEvents.last_x.ToString();
            entityEvents.last_y = UpdateReadByte(Offset.last_obj_event_y).Value;
            if (!textBox17.Focused) textBox17.Text = entityEvents.last_y.ToString();
            entityEvents.sum_x = UpdateReadValue(Offset.total_obj_event_x).Value;
            if (!textBox19.Focused) textBox19.Text = entityEvents.sum_x.ToString();
            entityEvents.sum_y = UpdateReadValue(Offset.total_obj_event_y).Value;
            if (!textBox18.Focused) textBox18.Text = entityEvents.sum_y.ToString();

            float finalRating_x, finalRating_y;
            if (entityEvents.amount > 0)
            {
                entityEvents.UpdateAverages();
                textBox15.Text = entityEvents.avg_x.ToString();
                finalRating_x = (((float)areaEvents.avg_x + entityEvents.avg_x) / 2);
                textBox16.Text = entityEvents.avg_y.ToString();
                finalRating_y = (((float)areaEvents.avg_y + entityEvents.avg_y) / 2);
            }
            else
            {
                textBox15.Text = "N";
                textBox16.Text = "A";
                finalRating_x = areaEvents.avg_x;
                finalRating_y = areaEvents.avg_y;
            }
            textBox22.Text = ((int)finalRating_x).ToString();
            textBox23.Text = ((int)finalRating_y).ToString();

            DrawDreamChart(finalRating_x, finalRating_y, blinkMarker, areaEvents, entityEvents);
            blinkMarker = !blinkMarker;

            if (UpdateReadValue(Offset.X).HasValue)
            {
                //Position
                if (!textBox5.Focused)
                {
                    textBox5.Text = UpdateReadValue(Offset.X).Value.ToString();
                }
                if (!textBox6.Focused)
                {
                    textBox6.Text = UpdateReadValue(Offset.Y).Value.ToString();
                }
                if (!textBox7.Focused)
                {
                    textBox7.Text = UpdateReadValue(Offset.Z).Value.ToString();
                }
            }

            //Chunk Position
            int? playerPointer = GetIngamePointer(0x8ab4c);
            if (playerPointer != null)
            {
                textBox26.Text = UpdateReadByte((int)playerPointer + 0xbc).Value.ToString();
                textBox27.Text = UpdateReadByte((int)playerPointer + 0xbd).Value.ToString();
                textBox28.Text = UpdateReadByte((int)playerPointer + 0xbe).Value.ToString();
                textBox29.Text = UpdateReadByte((int)playerPointer + 0xbf).Value.ToString();
            }


            //Cutscene Vars
            int cutsceneBank = UpdateReadByte(0x91698).Value;
            textBox33.Text = cutsceneBank.ToString();
            int cutsceneEntry = UpdateReadByte(0x9169a).Value;
            textBox34.Text = cutsceneEntry.ToString();

            if (cutsceneEntry < 0)
            {
                label26.Text = "none";
            }
            else if (cutsceneBank == -1)
            {
                label26.Text = String.Format("EVENT{0:D}.STR",cutsceneEntry + 1);
            }
            else
            {
                char character = (char)(cutsceneEntry + 65);
                string cutsceneFile = String.Format("SPDAY{0:D2}", cutsceneBank + 1) + character.ToString();
                if (cutsceneEntry < 2) 
                {
                    label26.Text = cutsceneFile + ".STR";
                }
                else
                {
                    label26.Text = cutsceneFile + ".TIM";
                }
            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.map);
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.map);
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.day);
            }
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.X);
            }
        }

        private void textBox7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.Z);
            }
        }

        private void textBox6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.Y);
            }
        }

        private void textBox24_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int currentDay = ReadVal(Offset.day).Value;
                int previousDay = currentDay - 1;
                if (currentDay <= 0)
                {
                    previousDay = 365;
                }
                ButtonSetByte((TextBox)sender, Offset.rating_charts + previousDay * 2);
            }
        }

        private void textBox25_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int currentDay = ReadVal(Offset.day).Value;
                int previousDay = currentDay - 1;
                if (currentDay <= 0)
                {
                    previousDay = 365;
                }
                ButtonSetByte((TextBox)sender, Offset.rating_charts + 1 + previousDay * 2);
            }
        }

        private void textBox10_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.total_event_x);
            }
        }

        private void textBox11_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.total_event_y);
            }
        }

        private void textBox8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.events);
            }
        }

        private void textBox9_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSetByte((TextBox)sender, Offset.last_event_x);
            }
        }

        private void textBox12_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSetByte((TextBox)sender, Offset.last_event_y);
            }
        }

        private void textBox19_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.total_obj_event_x);
            }
        }

        private void textBox18_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.total_obj_event_y);
            }
        }

        private void textBox21_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.obj_events);
            }
        }

        private void textBox20_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSetByte((TextBox)sender, Offset.last_obj_event_x);
            }
        }

        private void textBox17_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSetByte((TextBox)sender, Offset.last_obj_event_y);
            }
        }

        private void textBox32_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.dream_time_limit);
            }
        }

        private void textBox31_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.dream_timer);
            }
        }

        private void textBox35_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.RNG);
            }
        }

        private void textBox30_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.total_flashback_score);
            }
        }

    }
}