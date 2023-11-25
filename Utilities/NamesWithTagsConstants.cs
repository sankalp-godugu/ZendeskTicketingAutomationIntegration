using System.Collections.Generic;

namespace ZenDeskTicketProcessJob.Utilities
{
    public static class NamesWithTagsConstants
    {
        private static readonly Dictionary<string, string> carrierWithTagValues = new Dictionary<string, string>
        {
            {"636 Pipefitters", "carrier_636pipefitters"},
            {"98 Plumbers", "carrier_98plumbers"},
            {"Aetna Better Health - NY Medicaid", "carrier_aetnabetterhealth-nymedicaid"},
            {"Aetna Better Health - VA - Medicaid", "carrier_aetnabetterhealth-va-medicaid"},
            {"Aetna Better Health - Virginia Food - Medicaid", "carrier_aetnabetterhealth-virginiafood-medicaid"},
            {"Aetna Group Hearing - CT", "carrier_aetnagrouphearing-ct"},
            {"Aetna MA", "carrier_aetnama"},
            {"Aetna MMP", "carrier_aetnammp"},
            {"Aflac", "carrier_aflac"},
            {"AgeRight Advantage", "carrier_agerightadvantage"},
            {"Alignment Health Plan", "carrier_alignmenthealthplan"},
            {"AllyAlign", "carrier_allyalign"},
            {"Alterwood", "carrier_alterwood"},
            {"American Health Plan", "carrier_americanhealthplan"},
            {"Anthem Special Offers", "carrier_anthemspecialoffers"},
            {"Apex Health", "carrier_apexhealth"},
            {"AvMed", "carrier_avmed"},
            {"BayCarePlus", "carrier_baycareplus"},
            {"BCBS Arizona", "carrier_bcbsarizona"},
            {"BCBS NE", "carrier_bcbsne"},
            {"BCBS Rhode Island", "carrier_bcbsrhodeisland"},
            {"BCOM Health - Broward Community", "carrier_bcomhealth-browardcommunity"},
            {"Blue Cross Blue Shield Michigan", "carrier_bluecrossblueshieldmichigan"},
            {"BlueKC", "carrier_bluekc"},
            {"Brand New Day", "carrier_brandnewday"},
            {"Capital Blue Cross", "carrier_capitalbluecross"},
            {"Capital Health Plan", "carrier_capitalhealthplan"},
            {"CareFirst", "carrier_carefirst"},
            {"CareFirst Dual Prime - University of Maryland", "carrier_carefirstdualprime-universityofmaryland"},
            {"CareOregon", "carrier_careoregon"},
            {"CASEBP - Cayuga Health System", "carrier_casebp-cayugahealthsystem"},
            {"CDPHP", "carrier_cdphp"},
            {"Centene", "carrier_centene"},
            {"Central Health Plan", "carrier_centralhealthplan"},
            {"Chinese Community Health Plan - CCHP", "carrier_chinesecommunityhealthplan-cchp"},
            {"Clear Spring Health", "carrier_clearspringhealth"},
            {"Clever Care Health Plan", "carrier_clevercarehealthplan"},
            {"CNC", "carrier_cnc"},
            {"Commonwealth Care Alliance - CCA", "carrier_commonwealthcarealliance-cca"},
            {"CommuniCare Advantage", "carrier_communicareadvantage"},
            {"Community First Health Plan - CFHP", "carrier_communityfirsthealthplan-cfhp"},
            {"Community Health Choice", "carrier_communityhealthchoice"},
            {"CommunityCare of Oklahoma - CCOK", "carrier_communitycareofoklahoma-ccok"},
            {"Connecticare", "carrier_connecticare"},
            {"Cox Health", "carrier_coxhealth"},
            {"Dignity Health", "carrier_dignityhealth"},
            {"Doctors Healthcare Plan", "carrier_doctorshealthcareplan"},
            {"Elevance - HealthSun", "carrier_elevance-healthsun"},
            {"Elevance - Life Essentials", "carrier_elevance-lifeessentials"},
            {"Elevance - MA", "carrier_elevance-ma"},
            {"Elevance- AFC - Freedom - Optimum", "carrier_elevance-afc-freedom-optimum"},
            {"Elevance GRS", "carrier_elevancegrs"},
            {"EmblemHealth", "carrier_emblemhealth"},
            {"Emergient - Next Blue of North Dakota", "carrier_emergient-nextblueofnorthdakota"},
            {"Emergient - Vermont Blue Advantage", "carrier_emergient-vermontblueadvantage"},
            {"Emergient - Wellmark", "carrier_emergient-wellmark"},
            {"Employers Health", "carrier_employershealth"},
            {"eternalHealth", "carrier_eternalhealth"},
            {"Experience Health", "carrier_experiencehealth"},
            {"Fallon Health", "carrier_fallonhealth"},
            {"Florida Blue", "carrier_floridablue"},
            {"Florida Health Care Plan - FHCP", "carrier_floridahealthcareplan-fhcp"},
            {"Freedom Wholesale", "carrier_freedomwholesale"},
            {"Gateway", "carrier_gateway"},
            {"Geisinger", "carrier_geisinger"},
            {"GlobalHealth", "carrier_globalhealth"},
            {"Gold Kidney", "carrier_goldkidney"},
            {"HAP", "carrier_hap"},
            {"Health Alliance Medical Plan", "carrier_healthalliancemedicalplan"},
            {"Health Partner Plan - Jefferson Health Plans", "carrier_healthpartnerplan-jeffersonhealthplans"},
            {"Health Plan of San Mateo - HPSM", "carrier_healthplanofsanmateo-hpsm"},
            {"Healthfirst", "carrier_healthfirst"},
            {"HealthPartners MN", "carrier_healthpartnersmn"},
            {"HealthTeam Advantage", "carrier_healthteamadvantage"},
            {"Hometown Health", "carrier_hometownhealth"},
            {"Humana", "carrier_humana"},
            {"IBEW 241", "carrier_ibew241"},
            {"Imperial Health Plan", "carrier_imperialhealthplan"},
            {"Independent Health", "carrier_independenthealth"},
            {"LA Care", "carrier_lacare"},
            {"Lasso Health", "carrier_lassohealth"},
            {"Legacy Assurance", "carrier_legacyassurance"},
            {"Liberty Medicare Advantage", "carrier_libertymedicareadvantage"},
            {"Lumeris", "carrier_lumeris"},
            {"Mass Advantage", "carrier_massadvantage"},
            {"McLaren Health Plan", "carrier_mclarenhealthplan"},
            {"MD Wise", "carrier_mdwise"},
            {"MedMutual of Ohio", "carrier_medmutualofohio"},
            {"MetroPlus", "carrier_metroplus"},
            {"Molina - Iowa Medicaid", "carrier_molina-iowamedicaid"},
            {"Molina - Nebraska Medicaid", "carrier_molina-nebraskamedicaid"},
            {"Molina - Nevada Medicaid", "carrier_molina-nevadamedicaid"},
            {"Molina Healthcare - Medicare", "carrier_molinahealthcare-medicare"},
            {"MVP Health", "carrier_mvphealth"},
            {"National Vision Administrator - NVA", "carrier_nationalvisionadministrator-nva"},
            {"New Hanover Health Advantage", "carrier_newhanoverhealthadvantage"},
            {"Optima Health Medicaid", "carrier_optimahealthmedicaid"},
            {"Optima Health Medicare", "carrier_optimahealthmedicare"},
            {"PacificSource", "carrier_pacificsource"},
            {"Paramount Health", "carrier_paramounthealth"},
            {"Partners Health Plan", "carrier_partnershealthplan"},
            {"Provider Partners Health Plan - PPHP", "carrier_providerpartnershealthplan-pphp"},
            {"Sanford Health Plan", "carrier_sanfordhealthplan"},
            {"Santa Clara Family Heath Plan - SCFHP", "carrier_santaclarafamilyheathplan-scfhp"},
            {"Select Health", "carrier_selecthealth"},
            {"Sonder Health Plan", "carrier_sonderhealthplan"},
            {"Texas Independence Health Plan - TIHP", "carrier_texasindependencehealthplan-tihp"},
            {"UA Local 85", "carrier_ualocal85"},
            {"UAW Trust", "carrier_uawtrust"},
            {"University of Dayton", "carrier_universityofdayton"},
            {"UPMC", "carrier_upmc"},
            {"Verda Health Plan", "carrier_verdahealthplan"},
            {"Virginia Premier Medicaid", "carrier_virginiapremiermedicaid"},
            {"Virginia Premier Medicare", "carrier_virginiapremiermedicare"},
            {"VIVA Health", "carrier_vivahealth"},
            {"Zing Health", "carrier_zinghealth"},
            {"Other", "carrier_other"},
        };

        private static readonly Dictionary<int, string> requestorTypes = new Dictionary<int, string>
        {
            {1, "who_is_contacting_member"},
            {2, "who_is_contacting_member_representative"},
            {3, "who_is_contacting_health_plan"},
        };

        private static readonly Dictionary<string, long> ticketStatusIds = new Dictionary<string, long>
        {
            { "New", 16807567164695 },
            { "Reviewed", 19295269450263 },
            { "Closed Partially", 19294912565143 },
            { "In Review", 19294761414807 },
            { "Pending Processing", 19294744142359 },
            { "Closed",19294781411735  },
            { "Failed",19294928631191  },
            { "Closed Approved", 19317038480151  },
            { "Closed Declined", 19317055615639  },
        };


        private static readonly Dictionary<string, string> issueRelatedTypes = new Dictionary<string, string>
        {
            {"OTC/Healthy Foods", "csm-team_otc_healthy_foods"},
            {"PERS", "csm-team_pers"},
            {"Transportation", "csm-team_transportation"},
            {"FLEX Card", "csm-team_flex_card_services"},
            {"Grievances", "csm-team_grievances"},
            {"Supervisors", "csm-team_supervisor_callback"},
            {"General inquiry", "csm-team_general_inquiry"},
            {"Member Related issues", "csm-team_member_related_issues"}
        };

        public static long GetTagValueByTicketStatus(string ticketStatus)
        {
            if (ticketStatusIds.TryGetValue(ticketStatus?.ToString()?.TrimEnd(), out long tagValue))
            {
                return tagValue;
            }
            else
            {
                return 0;
            }
        }


        public static string GetTagValueByCarrierName(string carrierName)
        {
            if (carrierWithTagValues.TryGetValue(carrierName, out string tagValue))
            {
                return tagValue;
            }
            else
            {
                return "Carrier Not Found";
            }
        }
        public static string GetTagValueByRequestorType(int requestorType)
        {
            if (requestorTypes.TryGetValue(requestorType, out string tagValue))
            {
                return tagValue;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetTagValueByIssueRelated(string issueRelated)
        {
            if (issueRelatedTypes.TryGetValue(issueRelated, out string mapping))
            {
                return mapping;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
