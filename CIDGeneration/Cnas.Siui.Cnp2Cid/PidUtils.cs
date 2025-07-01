#region Copyright ©2011-2013, SIVECO Romania SA - All Rights Reserved
// ======================================================================
// Copyright ©2011-2013 SIVECO Romania SA - All Rights Reserved
// ======================================================================
// This file and its contents are protected by Romanian and International
// copyright laws. Unauthorized reproduction and/or distribution of all
// or any portion of the code contained herein is strictly prohibited
// and will result in severe civil and criminal penalties.
// Any violations of this copyright will be prosecuted
// to the fullest extent possible under law.
// ======================================================================
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.
// ======================================================================
#endregion

#region References
using System;
using System.Globalization;

#endregion

namespace Cnas.Siui.Cnp2Cid
{
    /// <summary>
    /// PID utils
    /// </summary>
    public static class PidUtils
    {
        #region Constants
        /// <summary>
        /// String considered to be a valid PID
        /// </summary>
        public const string NULL_PID = "0000000000000";
        /// <summary>
        /// The length of a PID
        /// </summary>
        public const int PID_LENGTH = 13;

        public static DateTime NULL_DATE
        {
            get
            {
                return DateTime.MinValue;
            }
        }

        const string defaultDateFormat = @"dd\/MM\/yyyy";

        /// <summary>
        /// Database value for male gender
        /// </summary>
        public const string MALE = "1";
        /// <summary>
        /// Database value for female gender
        /// </summary>
        public const string FEMALE = "2";
        /// <summary>
        /// Database value for unknown gender
        /// </summary>
        public const string UNSPECIFIED = "3";
        /// <summary>
        /// Database value for unknown gender
        /// </summary>
        public const string BISEXUAL = "4";

        #endregion

        #region Methods
        /// <summary>
        /// Check a string for a valid PID format.
        /// The NULL_PID is not a valid PID
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static bool IsValid( string pid )
        {
            return IsValid( pid, false );
        }

        /// <summary>
        /// Check a string for a valid PID format
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="allowNullPid">if the value of the parameter is true the NULL_PID is considered to be a valid PID</param>
        /// <returns></returns>
        public static bool IsValid( string pid, bool allowNullPid )
        {
            // an empty string or a null one is an invalid PID
            if( string.IsNullOrEmpty( pid ) )
            {
                return false;
            }
            // check the length of the string: 
            if( pid.Length != PidUtils.PID_LENGTH )
            {
                return false;
            }
            // the NULL_PID is a valid PID
            if( pid == PidUtils.NULL_PID )
            {
                return allowNullPid;
            }

            // check the gender
            if( PidUtils.ExtractGender( pid ) == UNSPECIFIED )
            {
                return false;
            }

            // check date format
            if( PidUtils.ExtractBirthDay( pid ) == NULL_DATE )
            {
                return false;
            }

            // TODO: check district value
            // ...

            // checking the control sum
            try
            {
                int[] controlValues = { 2, 7, 9, 1, 4, 6, 3, 5, 8, 2, 7, 9 };
                const int divideValue = 11;
                int controlValue = 0;
                for( int ind = 0; ind < controlValues.Length; ind++ )
                {
                    int nVal = Convert.ToInt32( pid[ind].ToString(), 10 );
                    controlValue += nVal * controlValues[ind];
                }
                int resultValue = controlValue % divideValue;
                if( resultValue == 10 )
                {
                    resultValue = 1;
                }

                int expectedControlValue = Convert.ToInt32( pid[pid.Length - 1].ToString(), 10 );

                if( resultValue != expectedControlValue )
                {
                    return false;
                }
            }
            catch( Exception )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extract gender from a PID string
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static string ExtractGender( string pid )
        {
            // for a null PID the gender is UNSPECIFIED
            if( string.IsNullOrEmpty( pid ) )
            {
                return UNSPECIFIED;
            }

            if( pid[0] == '0' || pid[0] == '9' )
            {
                return UNSPECIFIED;
            }

            try
            {
                if( int.Parse( pid[0].ToString( CultureInfo.InvariantCulture ) ) % 2 == 0 )
                {
                    return FEMALE;
                }
                else
                {
                    return MALE;
                }
            }
            catch( Exception )
            {
                return UNSPECIFIED;
            }
        }

        /// <summary>
        /// Extract birthday from a PID string
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static DateTime ExtractBirthDay( string pid )
        {
            if( string.IsNullOrEmpty( pid ) || pid.Length != 13 )
            {
                return NULL_DATE;
            }

            try
            {
                int centuryValue = int.Parse( pid.Substring( 0, 1 ) );
                int yearValue = int.Parse( pid.Substring( 1, 2 ) );
                int monthValue = int.Parse( pid.Substring( 3, 2 ) );
                int dayValue = int.Parse( pid.Substring( 5, 2 ) );
                bool isForeign = false;

                switch( centuryValue )
                {
                    case 1:
                    case 2:
                        yearValue += 1900;
                        break;
                    case 3:
                    case 4:
                        yearValue += 1800;
                        break;
                    case 5:
                    case 6:
                        yearValue += 2000;
                        break;
                    case 7:
                    case 8:
                        isForeign = true;
                        yearValue += 2000;
                        break;
                    default:
                        return NULL_DATE;
                }

                var birthDay = new DateTime( yearValue, monthValue, dayValue );
                if( isForeign == true && birthDay > DateTime.Today )
                {
                    birthDay = birthDay.AddYears( -100 );
                }
                return birthDay;
            }
            catch( Exception )
            {
                return NULL_DATE;
            }
        }

        public static bool CheckPIDGender( string pid, string gender )
        {
            return ( PidUtils.ExtractGender( pid ) == gender );
        }

        public static bool CheckPIDBirthDay( string pid, DateTime birthDay )
        {
            return ( PidUtils.ExtractBirthDay( pid ) == birthDay );
        }

        public static bool CheckPIDBirthDay( string pid, object birthDay )
        {
            if( birthDay is DateTime )
            {
                return ( PidUtils.ExtractBirthDay( pid ) == (DateTime)birthDay );
            }
            else
            {
                return ( PidUtils.ExtractBirthDay( pid ) == DateTime.ParseExact( birthDay.ToString(), defaultDateFormat, System.Globalization.CultureInfo.CurrentCulture ) );
            }
        }

        #endregion
    }
}