using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using CleansheetAPI.BusinessLayer;
using System.Data;
using System.Configuration;

public class Service : IService
{
    public double avgDistanceTravelledPerDay { get; set; }
    public int workingHours { get; set; }

    public double dieselRate { get; set; }
    
    //Long Haul = 0 & Local = 1
    public string typeOfTravel { get; set; }

    //Double Driver = 0 & Single Driver = 1
    public string driversType { get; set; }

    public int cleansheet_distance { get; set; }

    public int ageofTruck { get; set; }

    public string ddlCleanerReq { get; set; }

    public string vehicletypename { get; set; }

    public int qtyDriverCTC { get; set; }
    
    public int backHaul { get; set; }

    public double interestCost { get; set; }
    public double depreciation { get; set; }
    public double nationalRoadPermitForGoods  { get; set; }
    public double roadExpensesPerMonth { get; set; }
    public double insuranceCost { get; set; }

    public double monthlyTotalDriverAndCleanerMonthlySalaryAndWages { get; set; }

    public double batteryCostFixedCost { get; set; }
    public double gpsRentalFixedCost { get; set; }
    public double tarpaulinFixedCost { get; set; }
    public double mobileFixedCost { get; set; }
    public double fitnessCertificateFixedCost { get; set; }
    public double brandingCostFixedCost { get; set; }
    public double chainFixedCost { get; set; }
    public double woodenBlocksFixedCost { get; set; }

    public double loadingUnloadingTime { get; set; }
    public double noTripsPerMonth { get; set; }
    public double mileageWithLoad { get; set; }
    public double mileageWithoutLoad { get; set; }

    public double totalFixedCostMonthlyFixedCost { get; set; }
    public double ProfitMarginMonthlyTotalVarCost { get; set; }

    /// <summary>
    /// Get distance between origin & destination
    /// </summary>
    /// <param name="_origin">Origin</param>
    /// <param name="_destination">Destination</param>
    /// <returns></returns>
    public int GetDistance(string _origin, string _destination)
    {
        double distance = 0.0;
        //=============================
        BLCleansheet oBLCleansheet = new BLCleansheet();
        //=============================
        DataTable dt = oBLCleansheet.GetDistance(_origin, _destination);
        //=============================

        foreach (DataRow item in dt.Rows)
        {
            distance = Convert.ToDouble(item["DISTANCE"]);
        }

        if (distance > 0) distance = distance + (distance * (26.0 / 100.0));

        return Convert.ToInt32(distance);
    }

    public CleansheetRate GetCleansheetRate(string _origin, string _destination, string _vehicletype, double _dieselRate, double _loadingUnloadingTime, int _ageofTruck, int _backHaul, int _cleansheet_distance)
    {
        typeOfTravel = "0";
        driversType = "0";
        ageofTruck = _ageofTruck;
        ddlCleanerReq = "No";
        backHaul = _backHaul;
        dieselRate = _dieselRate;
        //noTripsPerMonth = _noTripsPerMonth;
        loadingUnloadingTime = _loadingUnloadingTime;
        cleansheet_distance = _cleansheet_distance;
        if (_cleansheet_distance <= 0)
        {
            _origin = _origin.Substring(0, _origin.IndexOf("(")).Trim();
            _destination = _destination.Substring(0, _destination.IndexOf("(")).Trim();

            if (_origin == null || _origin == "") _origin = string.Empty;
            if (_destination == null || _destination == "") _destination = string.Empty;
            //=================
            cleansheet_distance = GetDistance(_origin, _destination);
            //=================
        }
        //=================
        string _vehtypeid = GetVehicleTypeID(_vehicletype);
        //=================
        Cleansheet[] _vehicle_type_details = GetVehicleTypeDetails(_vehtypeid);
        //=================
        CleansheetRate rate = CalculateCleansheetRate(_vehicle_type_details);
        //=================
        return rate;
    }

    private CleansheetRate CalculateCleansheetRate(Cleansheet[] _vehicle_type_details)
    {
        CleansheetRate _rate = new CleansheetRate();
        Cleansheet _cleansheet = new Cleansheet();

        int oneWayDistance = cleansheet_distance;

        foreach (Cleansheet item in _vehicle_type_details)
        {
            _cleansheet = item;
        }
        //================
        vehicletypename = _cleansheet.vehicle_type_name;
        //================
        //Avg Distance Travelled / day
        getAvgDistanceTravelledPerDay(_cleansheet.avg_dist_trav_perday_local, _cleansheet.avg_dist_trav_perday_longhaul);
        //================
        //Transit time (in days)
        double mapDistanceCorrectionFactor = double.Parse(ConfigurationManager.AppSettings["mapDistanceCorrectionFactor"].ToString());
        oneWayDistance = Convert.ToInt32(Convert.ToDouble(oneWayDistance) * (1 + (mapDistanceCorrectionFactor / 100.0)));

        double transitTimeDays = Convert.ToDouble(oneWayDistance) / Convert.ToDouble(avgDistanceTravelledPerDay);
        if (transitTimeDays >= 1) transitTimeDays = Convert.ToDouble(Convert.ToDouble(Math.Ceiling(transitTimeDays * 10) / 10.0).ToString("0.#"));

        //------------------------------
        //No. of trips / month
        if (loadingUnloadingTime <= 0) loadingUnloadingTime = int.Parse(ConfigurationManager.AppSettings["loadingUnloadingTime"].ToString());

        if (noTripsPerMonth <= 0) noTripsPerMonth = roundDown(Convert.ToDouble(30 / ((transitTimeDays * 2) + loadingUnloadingTime)) * 2, 0) / 2;
        //================

        double ageofTruckMileage = 0.0;
        if (ageofTruck < 4)
        {
            ageofTruckMileage = 1;
        }
        else if (ageofTruck >= 8)
        {
            ageofTruckMileage = Convert.ToDouble(_cleansheet.mult_factor_age_of_veh_grtr_than_8_yrs) / 100.0;
        }
        else
        {
            ageofTruckMileage = Convert.ToDouble(_cleansheet.mult_factor_age_of_veh_grtr_than_4_yrs) / 100.0;
        }

        //Calc Mileage (Kms / ltr) (w/ load) & Mileage (Kms / ltr) (w/o load)
        mileageWithLoad = Convert.ToDouble(_cleansheet.mileage_with_load);
        mileageWithoutLoad = Convert.ToDouble(_cleansheet.mileage_wo_load);

        //Long Haul = 0 & Local = 1
        if (typeOfTravel == "1")
        {
            mileageWithLoad = mileageWithLoad * (Convert.ToDouble(_cleansheet.mult_factor_travel_type_local) / 100.0);
        }
        else if (typeOfTravel == "0")
        {
            mileageWithLoad = mileageWithLoad * (Convert.ToDouble(_cleansheet.mult_factor_travel_type_long_haul) / 100.0);
        }

        mileageWithLoad = mileageWithLoad * ageofTruckMileage;
        //================
        double tollCostRatePerKM = Convert.ToDouble(_cleansheet.toll_cost_rate_per_km);
        //================
        //------------------------------
        //Fixed & Variable Cost Vehicle Information
        //------------------------------
        var width = Convert.ToDouble(_cleansheet.vehicle_width) - 0.5;
        if (width < 0) width = 0.0;

        var length = Convert.ToDouble(_cleansheet.vehicle_length) - 0.25;
        if (length < 0) length = 0.0;

        var height = Convert.ToDouble(_cleansheet.vehicle_height) - 0.25;
        if (height < 0) height = 0.0;
        //------------------------------
        //Vehicle Fixed Cost Calculation 
        //------------------------------
        getVehicleFixedCostCalculation(_cleansheet);
        //------------------------------
        //Driver Fixed Cost Calculation 
        //------------------------------
        getDriverFixedCostCalculation(_cleansheet);
        //------------------------------
        //Other Fixed Cost Calculation 
        //------------------------------
        getOtherFixedCostCalculation(_cleansheet);
        //------------------------------
        //Total Fixed Cost per month 
        //------------------------------
        calcTotalFixedCostPerMonth(_cleansheet);
        //------------------------------
        //Fuel cost calculation
        //------------------------------
        calcFuelFixedCost(_cleansheet);
        //------------------------------
        var totalCostMonthly = totalFixedCostMonthlyFixedCost + ProfitMarginMonthlyTotalVarCost;
        //==============================
        //Cost Per Trip 
        var costPerTrip = totalCostMonthly / noTripsPerMonth;
        _rate.latest_rate = costPerTrip.ToString("0");

        return _rate;
    }

    #region "Cleansheet calculation"
    /// <summary>
    /// //Avg Distance Travelled / day
    /// </summary>
    /// <param name="local">Mileage Multiplying Factor for "Type of Travel" - Local</param>
    /// <param name="longhaul">Mileage Multiplying Factor for "Type of Travel" - Longhaul</param>
    private void getAvgDistanceTravelledPerDay(string local, string longhaul)
    {
        //Long Haul = 0 & Local = 1
        if (typeOfTravel == "1")
        {
            avgDistanceTravelledPerDay = Convert.ToInt32(local);
        }
        else if (typeOfTravel == "0")
        {
            avgDistanceTravelledPerDay = Convert.ToInt32(longhaul);
        }

        //Double Driver = 0 & Single Driver = 1
        if (driversType == "0")
        {
            //string.Format("{0:0.##}", 256.583); // "256.58"
            avgDistanceTravelledPerDay = Math.Ceiling(Convert.ToDouble(avgDistanceTravelledPerDay) * 1.67);
            workingHours = 24;
        }
        else
        {
            avgDistanceTravelledPerDay = Math.Ceiling(Convert.ToDouble(avgDistanceTravelledPerDay) * 1.0);
            workingHours = 10;
        }
    }

    private void getVehicleFixedCostCalculation(Cleansheet _cleansheet)
    {
        var costOfTractorOrTruck = Convert.ToDouble(_cleansheet.truck_cost);
        var costOfTrailorLOrContainerOrBody = Convert.ToDouble(_cleansheet.trailor_body_cost);
        var modificationCost = Convert.ToDouble(_cleansheet.modification_cost);
        var vehicleCustomisationCost = Convert.ToDouble(ConfigurationManager.AppSettings["vehicleCustomisationCost"]);
        var registration = Convert.ToDouble(_cleansheet.registration);
        var feesAndOthers = Convert.ToDouble(_cleansheet.fees_and_others);

        var totalCostOnRoad = Convert.ToDouble(costOfTractorOrTruck + costOfTrailorLOrContainerOrBody + modificationCost +
                            vehicleCustomisationCost + registration + feesAndOthers);

        //----------------------------
        var residualValueOfTheTruckFixedCost = Convert.ToDouble(_cleansheet.residual_value_at_EMI_end);
        var residualValueAfterEMIFixedCost = totalCostOnRoad * (residualValueOfTheTruckFixedCost / 100.0);
        var loanAmountPercFixedCost = Convert.ToDouble(_cleansheet.loan_amount);
        var loanAmtFixedCost = totalCostOnRoad * (loanAmountPercFixedCost / 100.0);
        var noOfYearsEMIFixedCost = Convert.ToDouble(_cleansheet.no_of_years_EMI);

        var rateOfInterestBankFixedCost = Convert.ToDouble(_cleansheet.rate_of_interest_ba_bank);
        var rateOfInterestPersonalFixedCost = Convert.ToDouble(_cleansheet.rate_of_interest_ba_personal);

        //Flat interest (Bank) % 
        var varpmt = PMT(rateOfInterestBankFixedCost / 100.0 / 12.0, noOfYearsEMIFixedCost * 12.0, totalCostOnRoad * -1.0, 0, 0);
        var nextcal = totalCostOnRoad / (noOfYearsEMIFixedCost * 12.0);
        var flatInterestBankFixedCost = Convert.ToDouble(((varpmt - nextcal) * 12 / totalCostOnRoad * 100.0).ToString("0.##"));
        
        //Flat interest (Personal) % 
        varpmt = PMT(rateOfInterestPersonalFixedCost / 100.0 / 12.0, noOfYearsEMIFixedCost * 12.0, totalCostOnRoad * -1.0, 0, 0);
        nextcal = totalCostOnRoad / (noOfYearsEMIFixedCost * 12.0);
        var flatInterestPersonalFixedCost = Convert.ToDouble(((varpmt - nextcal) * 12 / totalCostOnRoad * 100).ToString("0.##"));

        var interestCostMonthlyFixedCost = 0.0;
        if(ageofTruck < noOfYearsEMIFixedCost){
            
            var interstCostPMT = PMT((rateOfInterestBankFixedCost / 100.0) / 12.0, noOfYearsEMIFixedCost * 12, loanAmtFixedCost * -1.0, 0, 0);
            var interstCostNextCal = loanAmtFixedCost / (noOfYearsEMIFixedCost * 12.0);
            var interestCostMonthlyFixedCost1 = interstCostPMT - interstCostNextCal;

            var interstCostPMT1 = PMT((rateOfInterestPersonalFixedCost / 100.0) / 12.0, noOfYearsEMIFixedCost * 12, (totalCostOnRoad - loanAmtFixedCost) * -1.0, 0, 0);
            var interstCostNextCal1 = (totalCostOnRoad - loanAmtFixedCost) / (noOfYearsEMIFixedCost * 12.0);
            var interestCostMonthlyFixedCost2 = interstCostPMT1 - interstCostNextCal1;
            
            interestCostMonthlyFixedCost = interestCostMonthlyFixedCost1 + interestCostMonthlyFixedCost2;
        }
        else interestCostMonthlyFixedCost = 0.0; 
        
        interestCost = interestCostMonthlyFixedCost;
    
        var depreciationPerMonthMonthlyFixedCost = 0.0;
        var lifeOfTruck = Convert.ToDouble(_cleansheet.life_of_truck);

        if(ageofTruck >= lifeOfTruck){
            depreciationPerMonthMonthlyFixedCost = 0.0;
        }
        else {
            if(ageofTruck >= noOfYearsEMIFixedCost){
                depreciationPerMonthMonthlyFixedCost = residualValueAfterEMIFixedCost / ((lifeOfTruck - noOfYearsEMIFixedCost) * 12);                
            }
            else {
                depreciationPerMonthMonthlyFixedCost = loanAmtFixedCost / (noOfYearsEMIFixedCost * 12);
            }
        }
        depreciation = depreciationPerMonthMonthlyFixedCost;
        //----------------------------
        var natRoadPerExpMonthlyFixedCost = Convert.ToDouble(_cleansheet.NRP_expenses_for_goods);
        if(natRoadPerExpMonthlyFixedCost > 0) natRoadPerExpMonthlyFixedCost = natRoadPerExpMonthlyFixedCost / 12;
        nationalRoadPermitForGoods = natRoadPerExpMonthlyFixedCost;
    
        //Long Haul = 0 & Local = 1
        if (typeOfTravel == "1") natRoadPerExpMonthlyFixedCost = 0.0;
        
        var roadTAXExpMonthlyFixedCost = Convert.ToDouble(_cleansheet.road_tax_expenses_per_month);
        if(roadTAXExpMonthlyFixedCost > 0) roadTAXExpMonthlyFixedCost = roadTAXExpMonthlyFixedCost / 12;
        roadExpensesPerMonth = roadTAXExpMonthlyFixedCost;

        var insuranceCostPercentMonthlyFixedCost = Convert.ToDouble(_cleansheet.insurance_cost);
        
        var insuranceCostMonthlyFixedCost = Convert.ToDouble(_cleansheet.road_tax_expenses_per_month);
        if(insuranceCostMonthlyFixedCost > 0) insuranceCostMonthlyFixedCost = (totalCostOnRoad * insuranceCostPercentMonthlyFixedCost) / 100.0 / 12.0;
        insuranceCost = insuranceCostMonthlyFixedCost;
        //----------------------------
    }

    private void getDriverFixedCostCalculation(Cleansheet _cleansheet)
    {
        //Driver CTC
        //----------------------------
        var unitCostDriverCTC = Convert.ToDouble(_cleansheet.driver_ctc_per_month);
        
        qtyDriverCTC = 1;
        if(workingHours == 24) qtyDriverCTC = 2;
        //----------------------------
        //Cleaner Wages-Per/Month 
        var unitCostCleanerWagesPerMonth = Convert.ToDouble(_cleansheet.cleaner_ctc_per_month);
        
        var qtyCleanerWagesPerMonth = 0.0;
        if(ddlCleanerReq == "Yes") {
            if(workingHours == 24) qtyCleanerWagesPerMonth = 2;
            else qtyCleanerWagesPerMonth = 1;
        }
        //----------------------------
        //Driver Allowance / Day 
        var unitCostDriverAllowancePerDay = Convert.ToDouble(_cleansheet.driver_allowance_per_day);
        
        var qtyDriverAllowancePerDay = qtyDriverCTC * 30;
        //----------------------------
        //Cleaner Allowance / Day 
        var unitCostCleanerAllowancePerDay = Convert.ToDouble(_cleansheet.cleaner_allowance_per_day);
        
        var qtyCleanerAllowancePerDay = qtyCleanerWagesPerMonth * 30;
        //----------------------------
        //TOTAL Driver & Cleaner Monthly Yearly
        var monthlyDriverCTC = unitCostDriverCTC * qtyDriverCTC;
        
        var monthlyCleanerWagesPerMonth = unitCostCleanerWagesPerMonth * qtyCleanerWagesPerMonth;
        
        var monthlyDriverAllowancePerDay = unitCostDriverAllowancePerDay * qtyDriverAllowancePerDay;
        
        var monthlyCleanerAllowancePerDay = unitCostCleanerAllowancePerDay * qtyCleanerAllowancePerDay;
        //----------------------------
        //TOTAL Driver & Cleaner  
        monthlyTotalDriverAndCleanerMonthlySalaryAndWages = monthlyDriverCTC + monthlyCleanerWagesPerMonth
                                                            + monthlyDriverAllowancePerDay + monthlyCleanerAllowancePerDay;
        //----------------------------
    }

    private void getOtherFixedCostCalculation(Cleansheet _cleansheet)
    {
        //----------------------------
        //Other Fixed Cost Calculation
        //----------------------------
        var monthsBatteryCost = Convert.ToDouble(_cleansheet.average_battery_life);
        var unitCostBatteryCost = Convert.ToDouble(_cleansheet.battery_cost_for_one_battery);
        var noofbatteries = Convert.ToDouble(_cleansheet.no_of_batteries);
        var monthlyBatteryCost = (unitCostBatteryCost * noofbatteries) / monthsBatteryCost;
        batteryCostFixedCost = monthlyBatteryCost;
        //---------------------------- 
        //GPS rental charges per month 
        var gpsRentalChargesMonthly = Convert.ToDouble(_cleansheet.GPS_rental_charges_per_month);
        gpsRentalFixedCost = gpsRentalChargesMonthly;
        //---------------------------- 
        //Tarpaulin charges per month 
        var monthsTarpaulinChargesPerMonth = Convert.ToDouble(_cleansheet.tarpoulin_life);
        var unitCostTarpaulinChargesPerMonth = Convert.ToDouble(_cleansheet.tarpoulin_charges_per_month);
        
        var vehiclename = vehicletypename;
        var bodyType = vehiclename.Substring(vehiclename.Length - 2);
        
        var qtyTarpaulinChargesPerMonth = 0.0;
        if(bodyType == "HB" || bodyType == "FB") qtyTarpaulinChargesPerMonth = 1;
        
        //alert(qtyTarpaulinChargesPerMonth);
        
        var monthlyTarpaulinChargesPerMonth = (unitCostTarpaulinChargesPerMonth * qtyTarpaulinChargesPerMonth) / monthsTarpaulinChargesPerMonth;
        tarpaulinFixedCost = monthlyTarpaulinChargesPerMonth;
        //----------------------------
        //Mobile charges per month 
        var qtyDriver = qtyDriverCTC;
        var unitCostMobileChargesPerMonth = Convert.ToDouble(_cleansheet.mobile_charges_per_month);
        
        unitCostMobileChargesPerMonth = unitCostMobileChargesPerMonth * qtyDriver;
        mobileFixedCost = unitCostMobileChargesPerMonth;
        //----------------------------
        //Fitness Certificate charges 
        var monthsFitnessCertificateCharges = Convert.ToDouble(_cleansheet.fitness_certificate_life);
        var unitCostFitnessCertificateCharges = Convert.ToDouble(_cleansheet.fitness_certificate_charges);
        var qtyFitnessCertificateCharges = 1;
        var monthlyFitnessCertificateCharges = (unitCostFitnessCertificateCharges * qtyFitnessCertificateCharges) / monthsFitnessCertificateCharges;
        fitnessCertificateFixedCost = monthlyFitnessCertificateCharges;
        //---------------------------- 
        //Branding Cost 
        var monthsBrandingCost = Convert.ToDouble(_cleansheet.branding_life);
        var unitCostBrandingCost = Convert.ToDouble(_cleansheet.branding_cost);
        var qtyBrandingCost = 1;
        var monthlyBrandingCost = (unitCostBrandingCost * qtyBrandingCost) / monthsBrandingCost;
        brandingCostFixedCost = monthlyBrandingCost;
        //----------------------------
        //Chain 
        var monthsChain = Convert.ToDouble(_cleansheet.chain_life);
        var unitCostChain = Convert.ToDouble(_cleansheet.chain_cost);
        var qtyChain = 1;
        var monthlyChain = (unitCostChain * qtyChain) / monthsChain;
        chainFixedCost = monthlyChain;
        //----------------------------
        //Wooden Blocks
        var monthWoodenBlocks = Convert.ToDouble(_cleansheet.wooden_blocks_life);
        var unitCostWoodenBlocks = Convert.ToDouble(_cleansheet.wooden_blocks_cost);
        var qtyWoodenBlocks = 1;
        var monthlyWoodenBlocks = (unitCostWoodenBlocks * qtyWoodenBlocks) / monthWoodenBlocks;
        woodenBlocksFixedCost = monthlyWoodenBlocks;
        //---------------------------- 
    }

    private void calcTotalFixedCostPerMonth(Cleansheet _cleansheet)
    {
        //Fixed Cost - Vehicle
        //---------------------------- 
        var totalVehicleFixedCost = (interestCost + depreciation + nationalRoadPermitForGoods + roadExpensesPerMonth + insuranceCost);
        //---------------------------- 
        //Fixed Cost - Driver
        var totalDriverFixedCost = monthlyTotalDriverAndCleanerMonthlySalaryAndWages;
        //---------------------------- 
        //Fixed Cost - Other
        //----------------
        var otherFixedCost = (batteryCostFixedCost + gpsRentalFixedCost + tarpaulinFixedCost + mobileFixedCost 
                            + fitnessCertificateFixedCost + brandingCostFixedCost + chainFixedCost + woodenBlocksFixedCost);
        //---------------------------- 
        //Total Fixed Cost per month
        var monthlyTotalFixedCostPerMonthFixedCost = (totalVehicleFixedCost + totalDriverFixedCost + otherFixedCost) * (1 + (backHaul / 100.0)) / 2.0;
        //---------------------------- 
        //Working Capital Interest
        //----------------------------
        //var workingCapitalInterestMonthlyFixedCost = 0;//301;
        var paymentTerms = Convert.ToDouble(ConfigurationManager.AppSettings["paymentTerms"].ToString());
        var workingCapitalInterestPercFixedCost = Convert.ToDouble(ConfigurationManager.AppSettings["workingCapitalInterestPercFixedCost"].ToString());
        var workingCapitalInterestMonthlyFixedCost = monthlyTotalFixedCostPerMonthFixedCost * (paymentTerms / 365.0) * (workingCapitalInterestPercFixedCost / 100.0); 
        //---------------------------- 
        //Profit Margin on "Fixed Cost"
        //---------------------------- 
        var profitMarginPercFixedCost = Convert.ToDouble(_cleansheet.profit_margin_fixed_cost);
        var profitMarginOnFixedCostMonthlyFixedCost = (monthlyTotalFixedCostPerMonthFixedCost * profitMarginPercFixedCost) / 100.0;
        //---------------------------- 
        //Total Fixed Cost 
        //---------------------------- 
        totalFixedCostMonthlyFixedCost = monthlyTotalFixedCostPerMonthFixedCost + workingCapitalInterestMonthlyFixedCost + profitMarginOnFixedCostMonthlyFixedCost;
        //----------------------------  
    }

    private void calcFuelFixedCost(Cleansheet _cleansheet)
    {
        var noTripsPerMonthWithLoad = noTripsPerMonth;
        
        var oneWayDistance = Convert.ToDouble(cleansheet_distance);
        var mapDistanceCorrectionFactor = Convert.ToDouble(ConfigurationManager.AppSettings["mapDistanceCorrectionFactor"].ToString());
        oneWayDistance = oneWayDistance * (1+(mapDistanceCorrectionFactor/100.0));
                
        var noTripsPerMonthWithoutLoad = noTripsPerMonthWithLoad * (backHaul/100.0);
        
        //if(noTripsPerMonthWithoutLoad != 0) noTripsPerMonthWithoutLoad = 0.0;
                
        var travelledWithLoadMothly = noTripsPerMonthWithLoad * oneWayDistance;
        var  travelledWithoutLoadMothly = noTripsPerMonthWithoutLoad * oneWayDistance;
        
        //Fuel Cost (With Load)
        var fuelReqWithLoadVarCost = travelledWithLoadMothly / mileageWithLoad;
        var fuelCostWithLoadMonthlyVarCost = fuelReqWithLoadVarCost * dieselRate;
        
        //Fuel Cost (Without Load)
        var fuelReqWOVarCost = travelledWithoutLoadMothly/ mileageWithoutLoad;
        var fuelCostWOLoadMonthlyVarCost = fuelReqWOVarCost * dieselRate;
        //----------------------------
        //Maintnance Calculation
        var new_tyre_life = Convert.ToDouble(_cleansheet.new_tyre_life); 
        var new_tyre_cost = Convert.ToDouble(_cleansheet.new_tyre_cost); 
        var no_of_new_tyres = Convert.ToDouble(_cleansheet.no_of_new_tyres); 
        
        var retread_tyre_life = Convert.ToDouble(_cleansheet.retread_tyre_life); 
        var retread_tyre_cost = Convert.ToDouble(_cleansheet.retread_tyre_cost); 
        var no_of_retread_tyres = Convert.ToDouble(_cleansheet.no_of_retread_tyres); 
        
        //Annual
        var noOfYearsEMIFixedCost = Convert.ToDouble(_cleansheet.no_of_years_EMI);
        
        var travelledWithLoadYearly = travelledWithLoadMothly * 12.0;
        var travelledWithoutLoadYearly = travelledWithoutLoadMothly * 12.0;
        var totalTravelledYearly = travelledWithLoadYearly + travelledWithoutLoadYearly;
        
        var ifNewTyre1 = (new_tyre_cost * no_of_new_tyres);
        var ifNewTyre2 = totalTravelledYearly * noOfYearsEMIFixedCost;
        var totalTyreCost = new_tyre_life + retread_tyre_life;
        var freshTyreCostAnnualVarCost = 0.0;
        
        if(ageofTruck >= noOfYearsEMIFixedCost){
            freshTyreCostAnnualVarCost = ifNewTyre1 * ifNewTyre2 / totalTyreCost / noOfYearsEMIFixedCost;
        }
        else {
            freshTyreCostAnnualVarCost = ifNewTyre1 * (ifNewTyre2 - totalTyreCost) / totalTyreCost / noOfYearsEMIFixedCost;
        }
        if(freshTyreCostAnnualVarCost >= 0.0) {
            if(ageofTruck >= noOfYearsEMIFixedCost){
                freshTyreCostAnnualVarCost = ifNewTyre1 * ifNewTyre2 / totalTyreCost / noOfYearsEMIFixedCost;
            }
            else {
                freshTyreCostAnnualVarCost = ifNewTyre1 * (ifNewTyre2 - totalTyreCost) / totalTyreCost / noOfYearsEMIFixedCost;
            }
        }
        
        if(freshTyreCostAnnualVarCost < 0) freshTyreCostAnnualVarCost = 0.0;
        
        var freshTyreCostMonthlyVarCost = (freshTyreCostAnnualVarCost / 12.0);
        //---------------------------- 
        var ifRetreadTyre1 = (retread_tyre_cost * no_of_retread_tyres);
        var ifRetreadTyre2 = totalTravelledYearly * noOfYearsEMIFixedCost;
        //var totalTyreCost = new_tyre_life + retread_tyre_life;
        var retreadTyreCostAnnualVarCost = 0.0;
        
        if(ageofTruck >= noOfYearsEMIFixedCost){
            retreadTyreCostAnnualVarCost = ifRetreadTyre1 * ifRetreadTyre2 / totalTyreCost / noOfYearsEMIFixedCost;
        }
        else {
            retreadTyreCostAnnualVarCost = ifRetreadTyre1 * (ifRetreadTyre2 - new_tyre_life) / totalTyreCost / noOfYearsEMIFixedCost;
        }
        if(retreadTyreCostAnnualVarCost >= 0.0) {
            if(ageofTruck >= noOfYearsEMIFixedCost){
                retreadTyreCostAnnualVarCost = ifRetreadTyre1 * ifRetreadTyre2 / totalTyreCost / noOfYearsEMIFixedCost;
            }
            else {
                retreadTyreCostAnnualVarCost = ifRetreadTyre1 * (ifRetreadTyre2 - new_tyre_life) / totalTyreCost / noOfYearsEMIFixedCost;
            }
        }
        
        if(retreadTyreCostAnnualVarCost < 0) retreadTyreCostAnnualVarCost = 0.0;
        
        var retreadTyreCostMonthlyVarCost = (retreadTyreCostAnnualVarCost / 12.0);
        //---------------------------- 
        //Maintenance Cost
        var main_cost_rate_per_km = Convert.ToDouble(_cleansheet.main_cost_rate_per_km); 
        var main_cost_rate_per_anum = Convert.ToDouble(_cleansheet.main_cost_rate_per_anum); 
        
        var maintenancePercent = 150.0;
        //---------------------------- 
        var maintenanceForOldVehVarCost = 0.0;
        if(ageofTruck >= noOfYearsEMIFixedCost){            
            maintenanceForOldVehVarCost = (maintenancePercent / 100.0) * (travelledWithLoadMothly + travelledWithoutLoadMothly) * main_cost_rate_per_km;
        }
        else {
            maintenanceForOldVehVarCost = (travelledWithLoadMothly + travelledWithoutLoadMothly) * main_cost_rate_per_km;
        }
        var CostPerAnnum = main_cost_rate_per_anum / 12.0;
        if(CostPerAnnum > maintenanceForOldVehVarCost) maintenanceForOldVehVarCost = CostPerAnnum;
        //---------------------------- 
        //Other variable cost
        //var route_exp_rate_per_trip = parseFloat(cleansheet.find("route_exp_rate_per_trip); 
        var route_exp_rate_per_trip = Convert.ToDouble(ConfigurationManager.AppSettings["route_exp_rate_per_trip"].ToString());
        var route_exp_rate_per_km = Convert.ToDouble(_cleansheet.route_exp_rate_per_km); 
        
        var routeExpensesMonthlyVarCost = 0.0;
        
        //Long Haul = 0 & Local = 1
        if (typeOfTravel == "1"){
            routeExpensesMonthlyVarCost = route_exp_rate_per_trip * (noTripsPerMonthWithLoad + noTripsPerMonthWithoutLoad);
        }
        else {
            routeExpensesMonthlyVarCost = route_exp_rate_per_km * (travelledWithLoadMothly + travelledWithoutLoadMothly);
        }
        //---------------------------- 
        //Toll Cost 
        var tollCostPerTrip = Convert.ToDouble(ConfigurationManager.AppSettings["tollCostPerTrip"].ToString());
        
        //var toll_cost_rate_per_km = parseFloat(cleansheet.find("toll_cost_rate_per_km);
        var toll_cost_rate_per_km = Convert.ToDouble(_cleansheet.toll_cost_rate_per_km);
        
        var tollCostMonthlyVarCost = toll_cost_rate_per_km * (travelledWithLoadMothly + travelledWithoutLoadMothly);
        var tollCostMonthlyVarCost1 = tollCostPerTrip * (noTripsPerMonthWithLoad + noTripsPerMonthWithoutLoad);
        
        if(tollCostMonthlyVarCost1 > tollCostMonthlyVarCost) tollCostMonthlyVarCost = tollCostMonthlyVarCost1;
        //---------------------------- 
        //Total Variable Cost per month
        //---------------------------- 
        var TotalVariableCostPerMonth = fuelCostWithLoadMonthlyVarCost + fuelCostWOLoadMonthlyVarCost +
                                        freshTyreCostMonthlyVarCost + retreadTyreCostMonthlyVarCost + 
                                        maintenanceForOldVehVarCost + routeExpensesMonthlyVarCost + 
                                        tollCostMonthlyVarCost;
        //---------------------------- 
        //Working Capital Interest Variable Cost
        //----------------------------
        var paymentTerms = Convert.ToDouble(ConfigurationManager.AppSettings["paymentTerms"].ToString());
        var workingCapitalInterestPercVarCost = Convert.ToDouble(ConfigurationManager.AppSettings["workingCapitalInterestPercFixedCost"].ToString());
        var workingCapitalInterestMonthlyVarCost = TotalVariableCostPerMonth * (paymentTerms / 365) * (workingCapitalInterestPercVarCost / 100.0); 
        //----------------------------
        //Profit Margin on "Variable Cost"
        //----------------------------
        var profitMargin = Convert.ToDouble(_cleansheet.profit_margin);
        var profitMarginVariableCost = TotalVariableCostPerMonth * profitMargin / 100.0;
        //----------------------------
        //Total Variable Cost 
        //----------------------------
        ProfitMarginMonthlyTotalVarCost = TotalVariableCostPerMonth + workingCapitalInterestMonthlyVarCost
                        + profitMarginVariableCost;
        //----------------------------
    }
    #endregion

    #region "Common function"
    private double roundDown(double number, int decimals)
    {
        //if (decimals != 0)
        return (Math.Floor(number * Math.Pow(10, decimals)) / Math.Pow(10, decimals));
        //else return number;
    }

    private double PMT(double rate, double nper, double pv, double fv, double type)
    {
        fv = (fv > 0 ? fv : 0);
        type = (type > 0 ? type : 0);
        var invert = (pv < 0 ? true : false);
        pv = Math.Abs(pv);
        var v = ((-rate * (pv * Math.Pow(1.0 + rate, nper) + fv)) /
                        ((1.0 + rate * type) * (Math.Pow(1.0 + rate, nper) - 1))
                );
        return (invert ? -v : v);
    }
    #endregion

    #region "Get Vehicle Type Details"
    /// <summary>
    /// Get Vehicle Type ID by Vehicle Type Name
    /// </summary>
    /// <param name="vehicletype">Vehicle Type Name</param>
    /// <returns>Vehicle Type ID</returns>
    public string GetVehicleTypeID(string vehicletype)
    {
        string vehicletypeid = "0";
        //=============================
        BLCleansheet oBLCleansheet = new BLCleansheet();
        //=============================
        DataTable dt = oBLCleansheet.GetVehicleTypeByName(vehicletype);
        //=============================

        foreach (DataRow item in dt.Rows)
        {
            vehicletypeid = Convert.ToString(item["VEHICLETYPEID"]);
        }

        return vehicletypeid;
    }

    /// <summary>
    /// Get All Vehicle Type Details (Fixed Cost, Variable Cost)
    /// </summary>
    /// <param name="vehtypeid">Vehicle Type ID</param>
    /// <returns>Cleansheet[]: Vehicle Details</returns>
    private Cleansheet[] GetVehicleTypeDetails(string vehtypeid)
    {
        List<Cleansheet> returnObj = new List<Cleansheet>();

        using (BLCleansheet obj = new BLCleansheet())
        {
            obj._vehtypeid = Convert.ToInt64(vehtypeid);
            //============
            DataSet ds = obj.GetCleanSheetData();
            //============
            foreach (DataRow item in ds.Tables[0].Rows)
            {
                returnObj.Add(new Cleansheet()
                {
                    //vehicle_id = Convert.ToInt32(item["vehicle_id"]),
                    //vehicle_no = Convert.ToString(item["vehicle_no"]),
                    //modifieddt = Convert.ToString(item["modifieddt"]),
                    vehicle_type_id = Convert.ToString(item["vehicle_type_id"]),
                    vehicle_type_name = Convert.ToString(item["vehicle_type_name"]),
                    popular_name = Convert.ToString(item["popular_name"]),
                    model_name = Convert.ToString(item["model_name"]),
                    gross_veh_wt = Convert.ToString(item["gross_veh_wt"]),
                    carrying_capacity = Convert.ToString(item["carrying_capacity"]),
                    vehicle_length = Convert.ToString(item["vehicle_length"]),
                    vehicle_width = Convert.ToString(item["vehicle_width"]),
                    vehicle_height = Convert.ToString(item["vehicle_height"]),
                    capacity = Convert.ToString(item["capacity"]),
                    life_of_truck = Convert.ToString(item["life_of_truck"]),
                    mileage_with_load = Convert.ToString(item["mileage_with_load"]),
                    mileage_wo_load = Convert.ToString(item["mileage_wo_load"]),
                    avg_dist_trav_perday_longhaul = Convert.ToString(item["avg_dist_trav_perday_longhaul"]),
                    avg_dist_trav_perday_local = Convert.ToString(item["avg_dist_trav_perday_local"]),
                    truck_cost = Convert.ToString(item["truck_cost"]),
                    trailor_body_cost = Convert.ToString(item["trailor_body_cost"]),
                    modification_cost = Convert.ToString(item["modification_cost"]),
                    registration = Convert.ToString(item["registration"]),
                    fees_and_others = Convert.ToString(item["fees_and_others"]),
                    onRoad_cost = Convert.ToString(item["onRoad_cost"]),
                    residual_value_at_EMI_end = Convert.ToString(item["residual_value_at_EMI_end"]),
                    loan_amount = Convert.ToString(item["loan_amount"]),
                    NRP_expenses_for_goods = Convert.ToString(item["NRP_expenses_for_goods"]),
                    rate_of_interest_bank = Convert.ToString(item["rate_of_interest_bank"]),
                    rate_of_interest_personal = Convert.ToString(item["rate_of_interest_personal"]),
                    rate_of_interest_ba_bank = Convert.ToString(item["rate_of_interest_ba_bank"]),
                    rate_of_interest_ba_personal = Convert.ToString(item["rate_of_interest_ba_personal"]),
                    road_tax_expenses_per_month = Convert.ToString(item["road_tax_expenses_per_month"]),
                    insurance_cost = Convert.ToString(item["insurance_cost"]),
                    driver_ctc_per_month = Convert.ToString(item["driver_ctc_per_month"]),
                    driver_allowance_per_day = Convert.ToString(item["driver_allowance_per_day"]),
                    cleaner_ctc_per_month = Convert.ToString(item["cleaner_ctc_per_month"]),
                    cleaner_allowance_per_day = Convert.ToString(item["cleaner_allowance_per_day"]),
                    no_of_batteries = Convert.ToString(item["no_of_batteries"]),
                    battery_cost_for_one_battery = Convert.ToString(item["battery_cost_for_one_battery"]),
                    average_battery_life = Convert.ToString(item["average_battery_life"]),
                    tarpoulin_charges_per_month = Convert.ToString(item["tarpoulin_charges_per_month"]),
                    tarpoulin_life = Convert.ToString(item["tarpoulin_life"]),
                    mobile_charges_per_month = Convert.ToString(item["mobile_charges_per_month"]),
                    fitness_certificate_charges = Convert.ToString(item["fitness_certificate_charges"]),
                    fitness_certificate_life = Convert.ToString(item["fitness_certificate_life"]),
                    GPS_rental_charges_per_month = Convert.ToString(item["GPS_rental_charges_per_month"]),
                    branding_cost = Convert.ToString(item["branding_cost"]),
                    branding_life = Convert.ToString(item["branding_life"]),
                    chain_cost = Convert.ToString(item["chain_cost"]),
                    chain_life = Convert.ToString(item["chain_life"]),
                    wooden_blocks_cost = Convert.ToString(item["wooden_blocks_cost"]),
                    wooden_blocks_life = Convert.ToString(item["wooden_blocks_life"]),
                    profit_margin_fixed_cost = Convert.ToString(item["profit_margin_fixed_cost"]),
                    no_of_years_EMI = Convert.ToString(item["no_of_years_EMI"]),
                    mil_with_load = Convert.ToString(item["mil_with_load"]),
                    mil_without_load = Convert.ToString(item["mil_without_load"]),
                    mult_factor_age_of_veh = Convert.ToString(item["mult_factor_age_of_veh"]),
                    mult_factor_age_of_veh_grtr_than_4_yrs = Convert.ToString(item["mult_factor_age_of_veh_grtr_than_4_yrs"]),
                    mult_factor_age_of_veh_grtr_than_8_yrs = Convert.ToString(item["mult_factor_age_of_veh_grtr_than_8_yrs"]),
                    mult_factor_travel_type_long_haul = Convert.ToString(item["mult_factor_travel_type_long_haul"]),
                    mult_factor_travel_type_local = Convert.ToString(item["mult_factor_travel_type_local"]),
                    main_cost_rate_per_km = Convert.ToString(item["main_cost_rate_per_km"]),
                    main_cost_rate_per_anum = Convert.ToString(item["main_cost_rate_per_anum"]),
                    no_of_new_tyres = Convert.ToString(item["no_of_new_tyres"]),
                    new_tyre_life = Convert.ToString(item["new_tyre_life"]),
                    new_tyre_cost = Convert.ToString(item["new_tyre_cost"]),
                    no_of_retread_tyres = Convert.ToString(item["no_of_retread_tyres"]),
                    retread_tyre_life = Convert.ToString(item["retread_tyre_life"]),
                    retread_tyre_cost = Convert.ToString(item["retread_tyre_cost"]),
                    route_exp_rate_per_km = Convert.ToString(item["route_exp_rate_per_km"]),
                    route_exp_rate_per_trip = Convert.ToString(item["route_exp_rate_per_trip"]),
                    toll_cost_rate_per_km = Convert.ToString(item["toll_cost_rate_per_km"]),
                    profit_margin = Convert.ToString(item["profit_margin"]),
                });
            }
        }
        return returnObj.ToArray();
    }
    #endregion

    
}
