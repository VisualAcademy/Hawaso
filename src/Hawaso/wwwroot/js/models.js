var models = {
    Applicant: function () {
        return ({
            license_type: null, 
        });
    },

    Person: function () {
        return ({
            last_name: null,
            first_name: null,
            middle_name: null, 
            name_suffix: null, 

        });
    },


    DriverLicense: function () {
        return ({
            name: null,         // Optional display name or human-readable label for the license
            number: null,       // License number (e.g., driver license or professional license ID)
            state: null,        // Issuing state or region (e.g., "CA", "OK")
            start_date: null,   // Validity start date (optional)
            end_date: null,     // Validity end date (optional)
            type: null          // License type (e.g., "INSTANT_DRIVER", "PROFESSIONAL")
        });
    }


};
