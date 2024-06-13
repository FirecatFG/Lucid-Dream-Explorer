﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lucid_Dream_Explorer
{
    public partial class Form1 : Form
    {
        private static string[] maps = { "Bright Moon Cottage", "Pit & Temple", "Kyoto", "The Natural World", "Happy Town", "Violence District", "Moonlight Tower", "Temple Dojo", "Flesh Tunnels", "Clockwork Machines", "Long Hallway", "Sun Faces Heave", "Black Space", "Monument Park" };
        private static string[] sounds = { "00 - [Jangling]", "01 - [Opera Singer?]", "02 - [Jiggling]", "03 - Grass, Sand, H.T. Footstep", "04 - Train", "05 - Astronaut, Trumpeters", "06 - H.T. Squeak Footstep, Pterodactyl, Kissing Lips", "07 - Flowing Water (Natural World)", "08 - Link", "09 - H.T. Wood Footstep", "10 - Deep Rumbling", "11 - V.D. Dock Footstep", "12 - Wooden Bridge Footstep", "13 - [Tengu?]", "14 - Drumming (Kyoto)", "15 - H.T. Wet Footstep", "16 - [Tengu?]", "17 - [Clockwork Machine?]", "18 - Snarl (Lions, Dojo Dog)", "19 - Wings Flapping (Pterodactyl)", "20 - [Unknown]", "21 - Fetus Noise, Rocket Ship", "22 - Breathing (Sleeper in BMC)", "23 - BMC Birds Tweeting", "24 - [Long Hallway End?]", "25 - Horses Galloping (Natural World)", "26 - [UFO?]", "27 - Kyoto Grass Footstep", "28 - Generic Footstep (Tunnels, V.D., BMC Tile, NW Rock)", "29 - Water Footstep (NW River, Flesh Tunnel Slosh)" };

        private MemoryIO mio;
        private bool busy, blinkMarker;

        public Form1()
        {
            InitializeComponent();

            timer1.Start();
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

        private int DoEventAverage(int sum, int amt, int bonus)
        {
            int value = sum / amt + bonus / 3;

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

        private void DrawDreamChart(int x, int y, bool blink, int chunkX, int chunkY)
        {
            Pen ChunkPen = new Pen(Brushes.DodgerBlue, 3.0f);
            int center = pictureBox1.Width / 2; //Width == height here
            center -= 8; //Square center
            //Console.Out.WriteLine("Map: (" + x + ", "+ y + ")");

            Bitmap baseImg = Lucid_Dream_Explorer.Properties.Resources.DreamChart;
            Graphics g = Graphics.FromImage(baseImg);
            if (blink)
            {
                g.FillRectangle(System.Drawing.Brushes.Red, center + x * 16, center - y * 16, 16, 16);
            }
            /*else
            {
                g.FillRectangle(System.Drawing.Brushes.DarkRed, center + x * 16, center - y * 16, 16, 16);
            }*/
            g.DrawRectangle(ChunkPen, center + chunkX * 16 + 1, center - chunkY * 16 + 1, 14, 14);

            pictureBox1.Image = baseImg;
            ChunkPen.Dispose();
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

            //Day
            int currentDay = ReadVal(Offset.day).Value;
            if (!textBox3.Focused)
            {
                textBox3.Text = currentDay.ToString();
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
                previousDayMood_x = UpdateReadByte(0x916c0 + previousDay * 2).Value;
                textBox24.Text = previousDayMood_x.ToString();
            }
            if (!textBox25.Focused)
            {
                previousDayMood_y = UpdateReadByte(0x916c1 + previousDay * 2).Value;
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
            int amtChunks = UpdateReadValue(Offset.events).Value;
            if (!textBox8.Focused) textBox8.Text = amtChunks.ToString();
            int lastChunk_x = UpdateReadByte(Offset.last_event_x).Value;
            if (!textBox9.Focused) textBox9.Text = lastChunk_x.ToString();
            int lastChunk_y = UpdateReadByte(Offset.last_event_y).Value;
            if (!textBox12.Focused) textBox12.Text = lastChunk_y.ToString();
            int sumChunks_x = UpdateReadValue(Offset.total_event_x).Value;
            if (!textBox10.Focused) textBox10.Text = sumChunks_x.ToString();
            int sumChunks_y = UpdateReadValue(Offset.total_event_y).Value;
            if (!textBox11.Focused) textBox11.Text = sumChunks_y.ToString();
            int avgChunks_x = 0;
            int avgChunks_y = 0;

            if (amtChunks > 0)
            {
                avgChunks_x = DoEventAverage(sumChunks_x, amtChunks, lastChunk_x);
                avgChunks_y = DoEventAverage(sumChunks_y, amtChunks, lastChunk_y);
                textBox13.Text = avgChunks_x.ToString();
                textBox14.Text = avgChunks_y.ToString();
            }

            int amtTriggers = UpdateReadValue(Offset.obj_events).Value;
            if (!textBox21.Focused) textBox21.Text = amtTriggers.ToString();
            int lastTrigger_x = UpdateReadByte(Offset.last_obj_event_x).Value;
            if (!textBox20.Focused) textBox20.Text = lastTrigger_x.ToString();
            int lastTrigger_y = UpdateReadByte(Offset.last_obj_event_y).Value;
            if (!textBox17.Focused) textBox17.Text = lastTrigger_y.ToString();
            int sumTriggers_x = UpdateReadValue(Offset.total_obj_event_x).Value;
            if (!textBox19.Focused) textBox19.Text = sumTriggers_x.ToString();
            int sumTriggers_y = UpdateReadValue(Offset.total_obj_event_y).Value;
            if (!textBox18.Focused) textBox18.Text = sumTriggers_y.ToString();

            int finalRating_x, finalRating_y;
            if (amtTriggers > 0)
            {
                int avgTriggers_x = DoEventAverage(sumTriggers_x, amtTriggers, lastTrigger_x);
                int avgTriggers_y = DoEventAverage(sumTriggers_y, amtTriggers, lastTrigger_y);
                textBox15.Text = avgTriggers_x.ToString();
                textBox16.Text = avgTriggers_y.ToString();
                finalRating_x = ((avgChunks_x + avgTriggers_x) / 2);
                finalRating_y = ((avgChunks_y + avgTriggers_y) / 2);
            }
            else
            {
                textBox15.Text = "N";
                textBox16.Text = "A";
                finalRating_x = avgChunks_x;
                finalRating_y = avgChunks_y;
            }
            textBox22.Text = finalRating_x.ToString();
            textBox23.Text = finalRating_y.ToString();

            DrawDreamChart(finalRating_x, finalRating_y, blinkMarker, lastChunk_x, lastChunk_y);
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
                ButtonSetByte((TextBox)sender, 0x916c0 + previousDay * 2);
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
                ButtonSetByte((TextBox)sender, 0x916c1 + previousDay * 2);
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

        private void textBox30_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.total_flashback_score);
            }
        }

    }
}