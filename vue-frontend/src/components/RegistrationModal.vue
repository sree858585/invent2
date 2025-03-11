
<template>
    <div class="modal-overlay">

        <div class="modal">
            <h2>Create Your Account</h2>
            <form @submit.prevent="handleRegister" class="form-container">
                <!-- Two-column layout -->
                <div class="form-column">
                    <div class="form-group" v-for="(field, index) in inputFieldsLeft" :key="index">
                        <label :for="field.model">{{ field.label }} *</label>
                        <input :type="field.type"
                               v-model="form[field.model]"
                               :placeholder="field.placeholder"
                               :required="field.required"
                               :disabled="field.disabled || false" />

                    </div>
                </div>

                <div class="form-column">
                    <div class="form-group" v-for="(field, index) in inputFieldsRight" :key="index">
                        <label :for="field.model">{{ field.label }} *</label>
                        <input :type="field.type"
                               v-model="form[field.model]"
                               :placeholder="field.placeholder"
                               :required="field.required"
                               :disabled="field.disabled || false" />
                    </div>
                </div>
                <!-- Work Address Field (Full Width) -->
                <!--<div class="form-column-full">
                    <div class="form-group">
                        <label for="address">Work Address *</label>
                        <input type="text"
                               v-model="form.address"
                               placeholder="Enter your work address"
                               required />
                    </div>
                </div>-->


                <!-- Dropdown Fields -->
                <div class="form-column-full">
                    <div class="form-group">
                        <label>Password Recovery Question *</label>
                        <select v-model="form.passwordRecoveryQuestion" required>
                            <option disabled value="">Select a question</option>
                            <option>What was the name of your first pet?</option>
                            <option>What is your mother's maiden name?</option>
                            <option>What was your first school?</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>Password Recovery Answer *</label>
                        <input type="text"
                               v-model="form.passwordRecoveryAnswer"
                               placeholder="Enter your answer"
                               required />
                    </div>
                </div>

                <!-- Dropdowns -->
                <div class="form-column-full">
                    <div class="form-group" v-for="(dropdown, index) in dropdownFields" :key="index">
                        <label>{{ dropdown.label }} *</label>
                        <select v-model="form[dropdown.model]" required>
                            <option disabled value="">Select {{ dropdown.label }}</option>
                            <option v-for="option in dropdown.options" :key="option.code" :value="option.code">
                                {{ option.value }}
                            </option>

                        </select>
                    </div>
                </div>

                <!-- Submit and Cancel Buttons -->
                <div class="button-group">
                    <button type="submit" class="btn-primary">Register</button>
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script>
    import apiClient from '@/axios.js'; // Make sure the path is correct based on your project structure
    export default {
        name: "RegistrationModal",
        
        data() {
            return {
                form: {
                    password: "",
                    confirmPassword: "",
                    email: "",
                    confirmEmail: "",
                    altEmail: "",
                    firstName: "",
                    mi: "",
                    lastName: "",
                    workTitle: "",
                    organization: "",
                    address: "",
                    city: "",
                    state: "NY",
                    zip: "",
                    country: "USA",
                    workPhone: "",
                    passwordRecoveryQuestion: "",
                    passwordRecoveryAnswer: "",
                    workSetting: null,
                    education: null,
                    ethnicity: null,
                    race: null,
                    occupation: null,
                    yearsCurrentOccupation: null,
                },
                dropdownFields: [
                    { model: "workSetting", label: "Work Setting", options: [] },
                    { model: "education", label: "Education", options: [] },
                    { model: "ethnicity", label: "Ethnicity", options: [] },
                    { model: "race", label: "Race", options: [] },
                    { model: "occupation", label: "Occupation", options: [] },
                    { model: "yearsCurrentOccupation", label: "Years in Current Occupation", options: [] },
                ],
                inputFieldsLeft: [
                    { model: "email", label: "Primary Email", type: "email", placeholder: "Enter your primary email", required: true },
                    { model: "confirmEmail", label: "Confirm Primary Email", type: "email", placeholder: "Re-enter your email", required: true },
                    { model: "password", label: "Password", type: "password", placeholder: "Create a strong password", required: true },
                    { model: "confirmPassword", label: "Confirm Password", type: "password", placeholder: "Confirm your password", required: true },
                    { model: "firstName", label: "First Name", type: "text", placeholder: "Enter your first name", required: true },
                    { model: "mi", label: "Middle Initial (MI)", type: "text", placeholder: "M", required: false },
                    { model: "city", label: "City", type: "text", placeholder: "Enter your city", required: true },
                    { model: "address", label: "Work Address", type: "text", placeholder: "Enter your work address", required: true } // ✅ Added here
                ],

                inputFieldsRight: [
                    { model: "lastName", label: "Last Name", type: "text", placeholder: "Enter your last name", required: true },
                    { model: "workTitle", label: "Work Title", type: "text", placeholder: "Enter your work title", required: true },
                    { model: "altEmail", label: "Alternate Email", type: "email", placeholder: "Enter an alternate email", required: false },
                    { model: "organization", label: "Organization", type: "text", placeholder: "Enter your organization", required: true },
                    { model: "workPhone", label: "Work Phone", type: "text", placeholder: "Enter your work phone", required: true },
                    { model: "country", label: "Country", type: "text", placeholder: "USA", required: true, disabled: true },
                    { model: "state", label: "State", type: "text", placeholder: "NY", required: true, disabled: true },
                    { model: "zip", label: "Zip Code", type: "text", placeholder: "Enter your zip code", required: true }
                ],
            };
        },
        methods: {
            async handleRegister() {
                if (this.form.password !== this.form.confirmPassword) {
                    alert("Passwords do not match!");
                    return;
                }
                if (this.form.email !== this.form.confirmEmail) {
                    alert("Emails do not match!");
                    return;
                }
                console.log("Form submission data: ", this.form);  // ✅ Debug log

                try {
                    await apiClient.post("/registration/register", this.form);
                    alert("Registration Successful!");
                    this.$emit("close");
                } catch (error) {
                    console.error(error);
                    alert("Registration Failed. Please try again.");
                }
            },
            async fetchDropdownData() {
                try {
                    const response = await apiClient.get("/registration/lookups");
                    const lookups = response.data;

                    // Extracting values from the response
                    this.dropdownFields[0].options = lookups.workSettings.$values;
                    this.dropdownFields[1].options = lookups.educationLevels.$values;
                    this.dropdownFields[2].options = lookups.ethnicities.$values;
                    this.dropdownFields[3].options = lookups.races.$values;
                    this.dropdownFields[4].options = lookups.occupations.$values;
                    this.dropdownFields[5].options = lookups.yearsCurrentOccupation.$values;

                    // Debug logs to verify
                    console.log("Dropdown Data: ", this.dropdownFields);
                } catch (error) {
                    console.error("Error fetching dropdown data:", error);
                }
            },
            
        },
        mounted() {
            console.log("Registration modal mounted"); // Add this line 
            this.fetchDropdownData();
        },
    };</script>

<style scoped>
    /* Two-column Form Layout */
    .form-container {
        display: flex;
        flex-wrap: wrap;
        gap: 20px;
    }

    .form-column {
        flex: 1 1 45%;
    }

    .form-column-full {
        flex: 1 1 100%;
        margin-top: 20px;
    }

    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.8);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    .modal {
        background-color: #fff;
        padding: 40px;
        border-radius: 16px;
        width: 900px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.25);
    }

        .modal h2 {
            margin-bottom: 20px;
            font-size: 28px;
            text-align: center;
            color: #333;
        }

    .form-group {
        margin-bottom: 20px;
    }

    label {
        font-weight: bold;
        margin-bottom: 5px;
        display: block;
    }
    .form-column-full .form-group {
        width: 100%;
    }

    input,
    select {
        width: 100%;
        padding: 12px;
        border: 1px solid #ccc;
        border-radius: 6px;
        font-size: 14px;
        box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .button-group {
        display: flex;
        justify-content: space-between;
        margin-top: 30px;
    }

    .btn-primary {
        background-color: #3f51b5;
        color: white;
        padding: 10px 20px;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        width: 48%;
    }

        .btn-primary:hover {
            background-color: #303f9f;
        }

    .btn-secondary {
        background-color: #f5f5f5;
        color: #333;
        padding: 10px 20px;
        border: 1px solid #ccc;
        border-radius: 6px;
        cursor: pointer;
        width: 48%;
    }

        .btn-secondary:hover {
            background-color: #ddd;
        }
</style>
