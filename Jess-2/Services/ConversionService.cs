using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Rest;

using Colorful;
using Console = Colorful.Console;

using System;
using System.Timers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Windows.Forms;

using Microsoft.Extensions.DependencyInjection;

using Jessbot;
using Jessbot.Entities;
using Jessbot.Commands.Modules;
using Jessbot.Commands;

namespace Jessbot.Services
{
    public class ConversionService
    {
        // Initialize a "ByteHex" Dictionary, as well as its inverse.
        private readonly Dictionary<byte, string> ByteHex = new Dictionary<byte, string>();
        private readonly Dictionary<string, byte> HexByte = new Dictionary<string, byte>();

        private readonly DiscordSocketClient _bot;

        private readonly DatabaseService _db;
        private readonly CommandService _cmd;

        public ConversionService(DiscordSocketClient bot, DatabaseService databaseService, CommandService cmd)
        {
            _bot = bot;

            _db = databaseService;
            _cmd = cmd;

            // Iterate over the ByteHex Dictionary creation, plus its inverse.
            // NOTE: During development, this was an attempt to generate a 256-value Dictionary to
            //       avoid manual definition of its values. As of writing this, hopefully, it worked.
            for (byte i = 0; i <= 255; i++)
            {
                int XxR = i / 16; // First place, raw.
                int xXR = i % 16; // Second place, raw.

                string XxH = "0"; // First place, hex.
                string xXH = "0"; // Second place, hex.

                // Set the first place of this hex digit, based on the byte input.
                switch (XxR)
                { case 00: XxH = "0"; break; case 01: XxH = "1"; break; case 02: XxH = "2"; break; case 03: XxH = "3"; break;
                  case 04: XxH = "4"; break; case 05: XxH = "5"; break; case 06: XxH = "6"; break; case 07: XxH = "7"; break;
                  case 08: XxH = "8"; break; case 09: XxH = "9"; break; case 10: XxH = "A"; break; case 11: XxH = "B"; break;
                  case 12: XxH = "C"; break; case 13: XxH = "D"; break; case 14: XxH = "E"; break; case 15: XxH = "F"; break; }

                // Set the second place of this hex digit, based on the byte input.
                switch (xXR)
                { case 00: xXH = "0"; break; case 01: xXH = "1"; break; case 02: xXH = "2"; break; case 03: xXH = "3"; break;
                  case 04: xXH = "4"; break; case 05: xXH = "5"; break; case 06: xXH = "6"; break; case 07: xXH = "7"; break;
                  case 08: xXH = "8"; break; case 09: xXH = "9"; break; case 10: xXH = "A"; break; case 11: xXH = "B"; break;
                  case 12: xXH = "C"; break; case 13: xXH = "D"; break; case 14: xXH = "E"; break; case 15: xXH = "F"; break; }

                // Set the full hex code!
                string Hex = XxH + xXH;

                // Add to the Dictionary and its inverse.
                ByteHex.Add(i, Hex);
                HexByte.Add(Hex, i);

                // Just in case, if i = 255, break the loop.
                if (i == 255) { break; }
            }
        }

        public string ColorToHex(Color incoming)
        {
            // Pass the RGB into separate values so their hex values can be computed and strung together.
            byte Red = incoming.R;
            byte Grn = incoming.G;
            byte Blu = incoming.B;

            // Parse these values and string together.
            string outgoing = "#" + ByteHex[Red] + ByteHex[Grn] + ByteHex[Blu];

            // Send the color back.
            return outgoing;
        }

        public List<object> HexToColor(string incoming)
        {
            // Parse the incoming string to a more uniform value. Make sure it is valuated in uppercase.
            string RawHex = "";
            if (incoming.StartsWith("#")) { RawHex = incoming.Substring(1).ToUpper(); }
            else { RawHex = incoming.ToUpper(); }

            // Prepare for the next step.
            List<string> RawHexRGB = new List<string>();
            bool IsCompact = false;
            bool IncorrectFormat = false;

            // Prepare for final step now, if next step fails, you're screwed without it.
            List<object> outgoing = new List<object>();

            // Split the string into pieces. Check the length.
            if (RawHex.Length == 6 || RawHex.Length == 3) // Length is acceptable.
            {
                // Before continuing, ensure this string is in hexadecimal!!! Escape immediately if it is not!
                foreach (char c in RawHex)
                { if (c != '0' && c != '1' && c != '2' && c != '3' && c != '4' && c != '5' && c != '6' && c != '7' &&
                        c != '8' && c != '9' && c != 'A' && c != 'B' && c != 'C' && c != 'D' && c != 'E' && c != 'F')
                    { IncorrectFormat = true; outgoing.Add(new Color(0, 0, 0)); outgoing.Add(IncorrectFormat); return outgoing; } }

                if (RawHex.Length == 6) // Full-spectrum hex code.
                {
                    RawHexRGB.Add("" + RawHex[0] + RawHex[1]); // RED
                    RawHexRGB.Add("" + RawHex[2] + RawHex[3]); // GREEN
                    RawHexRGB.Add("" + RawHex[4] + RawHex[5]); // BLUE
                }
                else // More limited hex code, but should still work.
                {
                    RawHexRGB.Add("" + RawHex[0]); // RED
                    RawHexRGB.Add("" + RawHex[1]); // GREEN
                    RawHexRGB.Add("" + RawHex[2]); // BLUE

                    IsCompact = true; // This will let the final step know that you are working with a smaller hex chunk.
                }
            }
            else // Length is in an improper format. This could cause any number of errors. Escape!
            { IncorrectFormat = true; outgoing.Add(new Color(0, 0, 0)); outgoing.Add(IncorrectFormat); return outgoing; }

            // Now that you have the string in pieces, check if you're working with compact chunks.
            if (IsCompact) // You are working with compact hex chunks, this is a more complicated step.
            {
                // Generate a comparison chart.
                Dictionary<string, byte> HexChart = new Dictionary<string, byte>
                { { "0", 000 }, { "1", 017 }, { "2", 034 }, { "3", 051 }, { "4", 068 }, { "5", 085 }, { "6", 102 }, { "7", 119 },
                  { "8", 136 }, { "9", 153 }, { "A", 170 }, { "B", 187 }, { "C", 204 }, { "D", 221 }, { "E", 238 }, { "F", 255 } };

                // Now complete the step.
                byte Red = HexChart[RawHexRGB[0]]; // Red numeric value.
                byte Grn = HexChart[RawHexRGB[1]]; // Green numeric value.
                byte Blu = HexChart[RawHexRGB[2]]; // Blue numeric value.

                outgoing.Add(new Color(Red, Grn, Blu));
            }
            else // You are clear to grab values directly from the HexByte dictionary and pass into a Color.
            {
                byte Red = HexByte[RawHexRGB[0]]; // Red numeric value.
                byte Grn = HexByte[RawHexRGB[1]]; // Green numeric value.
                byte Blu = HexByte[RawHexRGB[2]]; // Blue numeric value.

                outgoing.Add(new Color(Red, Grn, Blu));
            }

            outgoing.Add(IncorrectFormat); // This is a "false" value, but you should still pass
            return outgoing;               // it in anyways, no use in wasting memory space.
        }
    }
}
