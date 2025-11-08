// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;



// Model for the SampleDataService. Replace with your own model.
public class SampleOrderDetail
{
	public long ProductID { get; set; }

	public string? ProductName { get; set; }

	public int Quantity { get; set; }

	public double Discount { get; set; }

	public string? QuantityPerUnit { get; set; }

	public double UnitPrice { get; set; }

	public string? CategoryName { get; set; }

	public string? CategoryDescription { get; set; }

	public double Total { get; set; }

	public string ShortDescription => $"Product ID: {ProductID} - {ProductName}";
}