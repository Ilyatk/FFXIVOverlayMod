﻿using System;
using System.Globalization;

namespace FFXIVOverlay
{
    public static class StringExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static System.Drawing.Color DefaultColorParse = System.Drawing.Color.FromArgb(255, 255, 255, 255);

        public static System.Drawing.Color ParseColor(this String str)
        {
            return str.ParseColor(DefaultColorParse);
        }

        public static System.Drawing.Color ParseColor(this String str, System.Drawing.Color defaultValue)
        {
            // #FF010203
            //  FF010203
            // #010203
            //  010203
            
            
            if (str.Length < 6 || str.Length > 9)
            {
                return defaultValue;
            }

            string s = str.TrimStart('#');

            try
            {
                if (str.Length == 6)
                {
                    return System.Drawing.Color.FromArgb(255,
                        int.Parse(s.Substring(0, 2), NumberStyles.HexNumber),
                        int.Parse(s.Substring(2, 2), NumberStyles.HexNumber),
                        int.Parse(s.Substring(4, 2), NumberStyles.HexNumber));
                }

                if (str.Length == 8)
                {
                    return System.Drawing.Color.FromArgb(
                        int.Parse(s.Substring(0, 2), NumberStyles.HexNumber),
                        int.Parse(s.Substring(2, 2), NumberStyles.HexNumber),
                        int.Parse(s.Substring(4, 2), NumberStyles.HexNumber),
                        int.Parse(s.Substring(6, 2), NumberStyles.HexNumber));
                }
            }
            catch (Exception)
            {
            }

            return defaultValue;
        }
    }
}