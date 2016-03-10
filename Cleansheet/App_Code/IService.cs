using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

[ServiceContract]
public interface IService
{

    [OperationContract]
    [WebInvoke(RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Bare,
        UriTemplate = "GetCleansheetRate?origin={_origin}&destination={_destination}&vehicletype={_vehicletype}&dieselRate={_dieselRate}&loadingUnloadingTime={_loadingUnloadingTime}&ageofTruck={_ageofTruck}&backHaul={_backHaul}&cleansheet_distance={_cleansheet_distance}",
        Method = "GET")]
    CleansheetRate GetCleansheetRate(string _origin, string _destination, string _vehicletype, double _dieselRate, double _loadingUnloadingTime, int _ageofTruck, int _backHaul, int _cleansheet_distance);

}

[DataContract(Name = "Cleansheet", Namespace = "")]
public class CleansheetRate
{
    [DataMember]
    public string latest_rate { get; set; }
}

[DataContract(Name = "Cleansheet", Namespace = "")]
public class Cleansheet
{
    [DataMember]
    public string vehicle_type_id { get; set; }
    [DataMember]
    public string vehicle_type_name { get; set; }
    [DataMember]
    public string popular_name { get; set; }
    [DataMember]
    public string model_name { get; set; }
    [DataMember]
    public string gross_veh_wt { get; set; }
    [DataMember]
    public string carrying_capacity { get; set; }
    [DataMember]
    public string vehicle_length { get; set; }
    [DataMember]
    public string vehicle_width { get; set; }
    [DataMember]
    public string vehicle_height { get; set; }
    [DataMember]
    public string capacity { get; set; }
    [DataMember]
    public string life_of_truck { get; set; }
    [DataMember]
    public string mileage_with_load { get; set; }
    [DataMember]
    public string mileage_wo_load { get; set; }
    [DataMember]
    public string avg_dist_trav_perday_longhaul { get; set; }
    [DataMember]
    public string avg_dist_trav_perday_local { get; set; }
    [DataMember]
    public string truck_cost { get; set; }
    [DataMember]
    public string trailor_body_cost { get; set; }
    [DataMember]
    public string modification_cost { get; set; }
    [DataMember]
    public string registration { get; set; }
    [DataMember]
    public string fees_and_others { get; set; }
    [DataMember]
    public string onRoad_cost { get; set; }
    [DataMember]
    public string residual_value_at_EMI_end { get; set; }
    [DataMember]
    public string loan_amount { get; set; }
    [DataMember]
    public string NRP_expenses_for_goods { get; set; }
    [DataMember]
    public string rate_of_interest_bank { get; set; }
    [DataMember]
    public string rate_of_interest_personal { get; set; }
    [DataMember]
    public string rate_of_interest_ba_bank { get; set; }
    [DataMember]
    public string rate_of_interest_ba_personal { get; set; }
    [DataMember]
    public string road_tax_expenses_per_month { get; set; }
    [DataMember]
    public string insurance_cost { get; set; }
    [DataMember]
    public string driver_ctc_per_month { get; set; }
    [DataMember]
    public string driver_allowance_per_day { get; set; }
    [DataMember]
    public string cleaner_ctc_per_month { get; set; }
    [DataMember]
    public string cleaner_allowance_per_day { get; set; }
    [DataMember]
    public string no_of_batteries { get; set; }
    [DataMember]
    public string battery_cost_for_one_battery { get; set; }
    [DataMember]
    public string average_battery_life { get; set; }
    [DataMember]
    public string tarpoulin_charges_per_month { get; set; }
    [DataMember]
    public string tarpoulin_life { get; set; }
    [DataMember]
    public string mobile_charges_per_month { get; set; }
    [DataMember]
    public string fitness_certificate_charges { get; set; }
    [DataMember]
    public string fitness_certificate_life { get; set; }
    [DataMember]
    public string GPS_rental_charges_per_month { get; set; }
    [DataMember]
    public string branding_cost { get; set; }
    [DataMember]
    public string branding_life { get; set; }
    [DataMember]
    public string chain_cost { get; set; }
    [DataMember]
    public string chain_life { get; set; }
    [DataMember]
    public string wooden_blocks_cost { get; set; }
    [DataMember]
    public string wooden_blocks_life { get; set; }
    [DataMember]
    public string profit_margin_fixed_cost { get; set; }
    [DataMember]
    public string no_of_years_EMI { get; set; }
    [DataMember]
    public string mil_with_load { get; set; }
    [DataMember]
    public string mil_without_load { get; set; }
    [DataMember]
    public string mult_factor_age_of_veh { get; set; }
    [DataMember]
    public string mult_factor_age_of_veh_grtr_than_4_yrs { get; set; }
    [DataMember]
    public string mult_factor_age_of_veh_grtr_than_8_yrs { get; set; }
    [DataMember]
    public string mult_factor_travel_type_long_haul { get; set; }
    [DataMember]
    public string mult_factor_travel_type_local { get; set; }
    [DataMember]
    public string main_cost_rate_per_km { get; set; }
    [DataMember]
    public string main_cost_rate_per_anum { get; set; }
    [DataMember]
    public string no_of_new_tyres { get; set; }
    [DataMember]
    public string new_tyre_life { get; set; }
    [DataMember]
    public string new_tyre_cost { get; set; }
    [DataMember]
    public string no_of_retread_tyres { get; set; }
    [DataMember]
    public string retread_tyre_life { get; set; }
    [DataMember]
    public string retread_tyre_cost { get; set; }
    [DataMember]
    public string route_exp_rate_per_km { get; set; }
    [DataMember]
    public string route_exp_rate_per_trip { get; set; }
    [DataMember]
    public string toll_cost_rate_per_km { get; set; }
    [DataMember]
    public string profit_margin { get; set; }
}
