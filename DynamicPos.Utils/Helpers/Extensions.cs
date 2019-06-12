using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DynamicPos.Utils.Helpers
{
    public static class Extensions
    {

        public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
        {
            var sortableList = new List<T>(collection);
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++)
            {
                collection.Move(collection.IndexOf(sortableList[i]), i);
            }
        }
         
        //input  : "1234"
        //output : byte arr[0]=0x12
        //output : byte arr[0]=0x34
        public static void ConvertStringToHexArray(this byte[] Out_byteArr)
        {
            string s = DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Year.ToString().Substring(2, 2).PadLeft(2, '0');
            byte[] ba = new byte[s.Length / 2];
            for (int i = 0; i < ba.Length; i++)
            {
                string temp = s.Substring(i * 2, 2);
                ba[i] = Convert.ToByte(temp, 16);
            }
            
            Array.Copy(ba, 0, Out_byteArr, 0, ba.Length);
            Array.Reverse(Out_byteArr);
        }

        public static void ConvertAscToBcdArray(this byte[] arr, string str)
        {
          //  arrLen = str.Length;
            Array.Copy(Encoding.Default.GetBytes(str), 0, arr, 0, str.Length);
        }


        public static byte[] AsciiToBCD(string Value)
        {
            byte[] tempbuf = new byte[32];
            byte[] tempbufbcd = new byte[16];
            Encoding asen = Encoding.Default;

            ushort fieldlen = (ushort)Value.Length;
            tempbuf = asen.GetBytes(Value);
            AsciiToBCD(ref tempbufbcd, (ushort)(fieldlen / 2), ref tempbuf, (ushort)fieldlen);

            return tempbufbcd;
        }

        public static bool AsciiToBCD(ref byte[] pu8PtOut, UInt16 u16LgOut, ref byte[] pu8PtIn, UInt16 u16LgIn)
        {
            UInt16 u16Index;

            for (u16Index = 0; u16Index < u16LgIn; ++u16Index)
            {
                if ((pu8PtIn[u16Index] < '0') || (pu8PtIn[u16Index] > '9'))
                {
                    return false;
                }
            }

            Pla_AscToHex(ref pu8PtOut, u16LgOut, ref pu8PtIn, u16LgIn);
            return true;
        } // End of Pla_AscToBcd

        public static byte Loc_ByteToNb(byte u8Byte)
        {
            if (u8Byte >= 0x41)
            {
                return Convert.ToByte((u8Byte - 0x07) & 0x0F);
            }//fin if.
            else
            {
                return Convert.ToByte(u8Byte & 0x0F);
            }//fin else.
        }

        public static void Pla_AscToHex(ref byte[] pu8PtOut, UInt16 u16LgOut, ref byte[] pu8PtIn, UInt16 u16LgIn)
        {
            UInt16 u16Index;       /* Index de travail */
            UInt16 inxOut = 0, inxIn = 0;       /* Index de travail */

            /* Conversion du champ */
            inxOut = (UInt16)(inxOut + u16LgOut - ((u16LgIn + 1) / 2));
            if ((u16LgIn % 2) == 1)
            {
                pu8PtOut[inxOut++] = (byte)(Loc_ByteToNb(pu8PtIn[inxIn++]) & 0x0F);
            }//fin if

            for (u16Index = 0; u16Index < (u16LgIn / 2); u16Index++)
            {
                pu8PtOut[inxOut] = (byte)((Loc_ByteToNb(pu8PtIn[inxIn++]) << 4) & 0xF0);
                // LCO 10/01/00: separation en 2 lignes pour le compilateur ARM
                //*pu8PtOut++ =(uint8)( *pu8PtOut + (Loc_ByteToNb(*pu8PtIn++) & 0x0F) ) ;
                pu8PtOut[inxOut] += (byte)(Loc_ByteToNb(pu8PtIn[inxIn++]) & 0x0F);
                inxOut++;
            }//fin for.

            return;
        }

        public static uint ConvertToUint(uint uintNumber)
        {
            string endValue = String.Format("{0}.{1:00}", uintNumber / 100, uintNumber % 100);

            return Convert.ToUInt32(endValue);
        }

        public static uint ConvertToUint(double doubleNumber)
        {
            string retStr = "";
            //string endValue = doubleNumber.ToString();
            string[] values = doubleNumber.ToString(CultureInfo.GetCultureInfo("tr-TR")).Split(',');
            if (values.Length == 2)
            {
                //virgülden sonra birşeyler var.

                if (values[1].Length == 1)
                {
                    //virgülden sonrasının uzunluğu bir 1.5 gibi. Buna bir sıfır ekleyeceğiz
                    retStr = values[0] + values[1] + "0";

                }
                else if (values.Length == 2)
                {
                    retStr = values[0] + values[1];
                }
                else
                {
                    retStr = values[0] + values[1].Substring(0, 2);
                }

            }
            else
            {
                //virgül yok ve uzunluk 1
                retStr = values[0] + "00";

            }


            return Convert.ToUInt32(retStr);
        }

        public static string FromStringddMMyy(this DateTime dateTime)
        {
           return DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Year.ToString().Substring(2, 2).PadLeft(2, '0');
        }

        public static string FromStringYyyyMMddHHmmSS(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"));
        }

        public static DateTime bcdToDateTime(byte[] dateArr, byte[] timeArr)
        {
            if (dateArr == null || timeArr == null || dateArr.Length != 3 || timeArr.Length != 3)
            {
                return DateTime.Now;
            }

            string[] dateStrArr = BitConverter.ToString(dateArr).Split('-');
            string[] timeStrArr = BitConverter.ToString(timeArr).Split('-');

            return new DateTime(
                Convert.ToInt32("20" + dateStrArr[0]),
                Convert.ToInt32(dateStrArr[1]),
                Convert.ToInt32(dateStrArr[2]),
                Convert.ToInt32(timeStrArr[0]),
                Convert.ToInt32(timeStrArr[1]),
                Convert.ToInt32(timeStrArr[2]));
        }

        public static string GetMD5Hash(string plaintext)
        {
            byte[] hash;
            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            }
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static double ConvertToDouble(ulong uLongNumber)
        {
            string value = Convert.ToString(uLongNumber);
            string retStr;
            if (value.Length < 2)
            {
                return 0;
            }
            else if (value.Length == 2)
            {
                retStr = "0." + value;
            }
            else
            {
                retStr = value.Substring(0, value.Length - 2) + "," + value[value.Length - 2] + value[value.Length - 1];
            }
            return Convert.ToDouble(retStr);
        }

        public static double ConvertToDouble(uint uintNumber)
        {
            string value = Convert.ToString(uintNumber);
            string retStr;
            if (value.Length < 2)
            {
                throw new Exception("");
            }
            else if (value.Length == 2)
            {
                retStr = "0." + value;
            }
            else
            {
                retStr = value.Substring(0, value.Length - 2) + "," + value[value.Length - 2] + value[value.Length - 1];
            }
            return Convert.ToDouble(retStr);
        }
        public static double ConvertToDouble(int intNumber)
        {
            string value = Convert.ToString(intNumber);
            string retStr;
            if (value.Length < 2)
            {
                throw new Exception("");
            }
            else if (value.Length == 2)
            {
                retStr = "0." + value;
            }
            else
            {
                retStr = value.Substring(0, value.Length - 2) + "," + value[value.Length - 2] + value[value.Length - 1];
            }
            return Convert.ToDouble(retStr);
        }

        public static string UIntToStringFullNumber(uint number)
        {
            if (number.ToString().Length >= 4)
            {
                string number1 = number.ToString().Substring(0, number.ToString().Length - 2);
                string number2 = number.ToString().Substring(number.ToString().Length - 2, 2);
                return number1 + "," + number2;
            }
            else if (number.ToString().Length == 3)
            {
                string number1 = number.ToString().Substring(0, 1);
                string number2 = number.ToString().Substring(1, 2);
                return number1 + "," + number2;
            }
            else
                return number.ToString();

        }

        public static uint ConvertToInt(int intNumber)
        {
            string endValue = String.Format("{0}.{1:00}", intNumber / 100, intNumber % 100);

            return Convert.ToUInt32(endValue);
        }

        public static int ConvertToInt(double doubleNumber)
        {
            string retStr = "";
            //string endValue = doubleNumber.ToString();
            string[] values = doubleNumber.ToString(CultureInfo.GetCultureInfo("tr-TR")).Split(',');
            if (values.Length == 2)
            {
                //virgülden sonra birşeyler var.

                if (values[1].Length == 1)
                {
                    //virgülden sonrasının uzunluğu bir 1.5 gibi. Buna bir sıfır ekleyeceğiz
                    retStr = values[0] + values[1] + "0";

                }
                else if (values.Length == 2)
                {
                    retStr = values[0] + values[1];
                }
                else
                {
                    retStr = values[0] + values[1].Substring(0, 2);
                }

            }
            else
            {
                //virgül yok ve uzunluk 1
                retStr = values[0] + "00";

            }


            return Convert.ToInt32(retStr);
        }

    }
}
