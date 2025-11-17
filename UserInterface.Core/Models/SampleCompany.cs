// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface.Core
// File:  SampleCompany.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;


// Model for the SampleDataService. Replace with your own model.
public class SampleCompany
{


    public string? CompanyID { get; set; }

    public string? CompanyName { get; set; }

    public string? ContactName { get; set; }

    public string? ContactTitle { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public ICollection<SampleOrder>? Orders { get; set; }


}