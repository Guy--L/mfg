update Tag set isarchived = 1 where name in (
'Casing_tem_Hlimit','Casing_tem_Llimit', 'casing_temp2_f_pv', 'casing_temp3_f_pv',
'z3a_hum_%_pv', 'moisture_sp', 'moisture_pv_flt',
'Inside_Wetness_Hlimit', 'Inside_Wetness_Llimit', 'Inside_Casing_wet_flt',
'z1a_temp_f_sp', 'z1a_temp_f_pv', 'z1b_temp_f_sp', 'z1b_temp_f_pv', 'z1c_temp_f_sp', 'z1c_temp_f_pv', 
'line_fpm_pv', 
'extruder_speed_rpm_pv', 'water_godet_rpm_pv', 'plasticizer_godet_rpm_pv', 'belt_speed_fpm_pv', 'metering_pump_rpm_pv'
)
