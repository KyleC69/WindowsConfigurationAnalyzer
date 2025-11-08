// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Runtime.InteropServices;
using System.Text;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;



public class RuntimeHelper
{
	public static bool IsMsix
	{
		get
		{
			var length = 0;

			return GetCurrentPackageFullName(ref length, null) != 15700L;
		}
	}





	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);
}