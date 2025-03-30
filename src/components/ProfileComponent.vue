<template>
    <div class="profile-container">
        <h2>User Profile</h2>
        <button class="edit-btn" @click="openEditModal">Edit Profile</button>

        <div v-if="user" class="profile-card">
            <table class="modern-table">
                <tbody>
                    <tr>
                        <th>Name</th>
                        <td>{{ user.firstName }} {{ user.mi }} {{ user.lastName }}</td>
                    </tr>
                    <tr>
                        <th>Email</th>
                        <td>{{ user.email }}</td>
                    </tr>
                    <tr>
                        <th>Alternate Email</th>
                        <td>{{ user.altEmail }}</td>
                    </tr>
                    <tr>
                        <th>Title</th>
                        <td>{{ user.title }}</td>
                    </tr>
                    <tr>
                        <th>Organization</th>
                        <td>{{ user.organization }}</td>
                    </tr>
                    <tr>
                        <th>Country</th>
                        <td>{{ user.country }}</td>
                    </tr>
                    <tr>
                        <th>Work Setting</th>
                        <td>{{ user.workSetting || "Unknown" }}</td>
                    </tr>
                    <tr>
                        <th>Education</th>
                        <td>{{ user.education || "Unknown" }}</td>
                    </tr>
                    <tr>
                        <th>Ethnicity</th>
                        <td>{{ user.ethnicity || "Unknown" }}</td>
                    </tr>
                    <tr>
                        <th>Race</th>
                        <td>{{ user.race || "Unknown" }}</td>
                    </tr>
                    <tr>
                        <th>Occupation</th>
                        <td>{{ user.occupation || "Unknown" }}</td>
                    </tr>
                    <tr>
                        <th>Years in Current Occupation</th>
                        <td>{{ user.yearsCurrentOccupation || "Unknown" }}</td>
                    </tr>
                    <tr>
                        <th>Address</th>
                        <td>{{ user.address }}</td>
                    </tr>
                    <tr>
                        <th>City</th>
                        <td>{{ user.city }}</td>
                    </tr>
                    <tr>
                        <th>State</th>
                        <td>{{ user.state }}</td>
                    </tr>
                    <tr>
                        <th>Zip</th>
                        <td>{{ user.zip }}</td>
                    </tr>
                    <tr>
                        <th>Phone</th>
                        <td>{{ user.phone }}</td>
                    </tr>
                    <tr>
                        <th>Cell Phone</th>
                        <td>{{ user.cellPhone }}</td>
                    </tr>
                    <tr>
                        <th>Work Phone</th>
                        <td>{{ user.workPhone }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
        <p v-else>Loading user details...</p>

        <!-- 🔹 Edit Profile Modal -->
        <div v-if="showEditModal" class="modal-overlay">
            <div class="modal modal-wide">
                <h3>Edit Profile</h3>
                <form @submit.prevent="updateUserProfile">
                    <div class="form-container">
                        <div class="form-group">
                            <label>First Name</label>
                            <input type="text" v-model="editUser.firstName" required />
                        </div>
                        <div class="form-group">
                            <label>Middle Initial</label>
                            <input type="text" v-model="editUser.mi" />
                        </div>
                        <div class="form-group">
                            <label>Last Name</label>
                            <input type="text" v-model="editUser.lastName" required />
                        </div>
                        <div class="form-group">
                            <label>Email</label>
                            <input type="email" v-model="editUser.email" required />
                        </div>
                        <div class="form-group">
                            <label>Alternate Email</label>
                            <input type="email" v-model="editUser.altEmail" />
                        </div>
                        <div class="form-group">
                            <label>Title</label>
                            <input type="text" v-model="editUser.title" />
                        </div>
                        <div class="form-group">
                            <label>Organization</label>
                            <input type="text" v-model="editUser.organization" />
                        </div>
                        <div class="form-group">
                            <label>Country</label>
                            <input type="text" v-model="editUser.country" />
                        </div>
                        <div class="form-group">
                            <label>Work Setting</label>
                            <select v-model="editUser.workSetting">
                                <option value="" disabled>Select Work Setting</option>
                                <option v-for="(value, key) in lookupMappings.workSettings" :key="key" :value="key">
                                    {{ value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Education</label>
                            <select v-model="editUser.education">
                                <option value="" disabled>Select Education</option>
                                <option v-for="(value, key) in lookupMappings.educations" :key="key" :value="key">
                                    {{ value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Ethnicity</label>
                            <select v-model="editUser.ethnicity">
                                <option value="" disabled>Select Ethnicity</option>
                                <option v-for="(value, key) in lookupMappings.ethnicities" :key="key" :value="key">
                                    {{ value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Race</label>
                            <select v-model="editUser.race">
                                <option value="" disabled>Select Race</option>
                                <option v-for="(value, key) in lookupMappings.races" :key="key" :value="key">
                                    {{ value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Occupation</label>
                            <select v-model="editUser.occupation">
                                <option value="" disabled>Select Occupation</option>
                                <option v-for="(value, key) in lookupMappings.occupations" :key="key" :value="key">
                                    {{ value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Years in Current Occupation</label>
                            <select v-model="editUser.yearsCurrentOccupation">
                                <option value="" disabled>Select Years in Occupation</option>
                                <option v-for="(value, key) in lookupMappings.yearsCurrentOccupation" :key="key" :value="key">
                                    {{ value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Address</label>
                            <input type="text" v-model="editUser.address" />
                        </div>
                        <div class="form-group">
                            <label>City</label>
                            <input type="text" v-model="editUser.city" />
                        </div>
                        <div class="form-group">
                            <label>State</label>
                            <input type="text" v-model="editUser.state" />
                        </div>
                        <div class="form-group">
                            <label>Zip</label>
                            <input type="text" v-model="editUser.zip" />
                        </div>
                        <div class="form-group">
                            <label>Phone</label>
                            <input type="text" v-model="editUser.phone" />
                        </div>
                        <div class="form-group">
                            <label>Cell Phone</label>
                            <input type="text" v-model="editUser.cellPhone" />
                        </div>
                        <div class="form-group">
                            <label>Work Phone</label>
                            <input type="text" v-model="editUser.workPhone" />
                        </div>
                    </div>
                    <button type="submit" class="btn-save">Save Changes</button>
                    <button type="button" class="btn-cancel" @click="showEditModal = false">Cancel</button>
                </form>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios.js";

export default {
    props: ["id"],
    data() {
        return {
            user: null,
            editUser: {},
            showEditModal: false,
            lookupMappings: {
                workSettings: {},
                educations: {},
                ethnicities: {},
                races: {},
                occupations: {},
                yearsCurrentOccupation: {}
            }
        };
    },
    async mounted() {
        const userId = localStorage.getItem("userId");
        if (!userId) {
            alert("User ID not found. Please log in again.");
            return;
        }

        try {
            console.log("📢 Fetching user profile for ID:", userId);
            const response = await apiClient.get(`/user/${userId}`);
            this.user = response.data;
            console.log("✅ Received user data:", this.user);

            console.log("📢 Fetching lookup data...");
            const lookupResponse = await apiClient.get("/registration/lookups");
            console.log("✅ Received lookup data:", lookupResponse.data);

            if (!lookupResponse.data) {
                throw new Error("Lookup data is empty");
            }

            // 🔥 Convert lookup data into {Code: Value} mappings
            this.lookupMappings.workSettings = this.createLookupMap(lookupResponse.data.workSettings);
            this.lookupMappings.educations = this.createLookupMap(lookupResponse.data.educationLevels);
            this.lookupMappings.ethnicities = this.createLookupMap(lookupResponse.data.ethnicities);
            this.lookupMappings.races = this.createLookupMap(lookupResponse.data.races);
            this.lookupMappings.occupations = this.createLookupMap(lookupResponse.data.occupations);
            this.lookupMappings.yearsCurrentOccupation = this.createLookupMap(lookupResponse.data.yearsCurrentOccupation);

            console.log("✅ Lookup mappings:", this.lookupMappings);

           
        } catch (error) {
            console.error("❌ Error fetching user details or lookup data:", error);
            alert("Failed to fetch user details or lookup data!");
        }
    },

    methods: {
        async openEditModal() {
    if (!this.user) {
        alert("User data is not available.");
        return;
    }

    // Ensure lookup data is available
    if (Object.keys(this.lookupMappings.workSettings).length === 0) {
        console.log("📢 Lookup data is missing. Fetching again...");
        try {
            const lookupResponse = await apiClient.get("/registration/lookups");
            if (!lookupResponse.data) {
                throw new Error("Lookup data is empty");
            }

            // 🔥 Convert lookup data into {Code: Value} mappings
            this.lookupMappings.workSettings = this.createLookupMap(lookupResponse.data.workSettings);
            this.lookupMappings.educations = this.createLookupMap(lookupResponse.data.educationLevels);
            this.lookupMappings.ethnicities = this.createLookupMap(lookupResponse.data.ethnicities);
            this.lookupMappings.races = this.createLookupMap(lookupResponse.data.races);
            this.lookupMappings.occupations = this.createLookupMap(lookupResponse.data.occupations);
            this.lookupMappings.yearsCurrentOccupation = this.createLookupMap(lookupResponse.data.yearsCurrentOccupation);
            console.log("✅ Lookup data reloaded.");
        } catch (error) {
            console.error("❌ Error fetching lookup data:", error);
            alert("Failed to fetch lookup data. Please try again.");
            return;
        }
    }

    // Convert user values to lookup keys for dropdown selection
    this.editUser = {
        ...this.user,
        workSetting: Object.keys(this.lookupMappings.workSettings).find(
            key => this.lookupMappings.workSettings[key] === this.user.workSetting
        ) || "",
        education: Object.keys(this.lookupMappings.educations).find(
            key => this.lookupMappings.educations[key] === this.user.education
        ) || "",
        ethnicity: Object.keys(this.lookupMappings.ethnicities).find(
            key => this.lookupMappings.ethnicities[key] === this.user.ethnicity
        ) || "",
        race: Object.keys(this.lookupMappings.races).find(
            key => this.lookupMappings.races[key] === this.user.race
        ) || "",
        occupation: Object.keys(this.lookupMappings.occupations).find(
            key => this.lookupMappings.occupations[key] === this.user.occupation
        ) || "",
        yearsCurrentOccupation: Object.keys(this.lookupMappings.yearsCurrentOccupation).find(
            key => this.lookupMappings.yearsCurrentOccupation[key] === this.user.yearsCurrentOccupation
        ) || ""
    };

    this.showEditModal = true;
},
        createLookupMap(lookupObject) {
    if (!lookupObject || !lookupObject.$values || !Array.isArray(lookupObject.$values)) {
        console.error("🚨 Invalid lookup data:", lookupObject);
        return {};
    }

    const lookupMap = {};
    lookupObject.$values.forEach(item => {
        if (!item || !item.code || !item.value) {
            console.error("🚨 Skipping invalid lookup item:", item);
            return;
        }
        lookupMap[item.code.toString()] = item.value;
    });

    console.log("✅ Created Lookup Map:", lookupMap);
    return lookupMap;
},

        async updateUserProfile() {
    try {
        const userId = localStorage.getItem("userId");
        if (!userId) {
            alert("User ID not found.");
            return;
        }

        const payload = {
            ...this.editUser,
            workSetting: this.editUser.workSetting || null,
            education: this.editUser.education || null,
            ethnicity: this.editUser.ethnicity || null,
            race: this.editUser.race || null,
            occupation: this.editUser.occupation || null,
            yearsCurrentOccupation: this.editUser.yearsCurrentOccupation || null
        };

        console.log("🚀 Sending Update Payload:", JSON.stringify(payload, null, 2));

        const response = await apiClient.put(`/user/${userId}`, payload);

        console.log("✅ Profile updated successfully:", response.data);
        alert("Profile updated successfully!");

        // ✅ Update the main user data
        this.user = {
            ...this.editUser,
            workSetting: this.lookupMappings.workSettings[this.editUser.workSetting] || "Unknown",
            education: this.lookupMappings.educations[this.editUser.education] || "Unknown",
            ethnicity: this.lookupMappings.ethnicities[this.editUser.ethnicity] || "Unknown",
            race: this.lookupMappings.races[this.editUser.race] || "Unknown",
            occupation: this.lookupMappings.occupations[this.editUser.occupation] || "Unknown",
            yearsCurrentOccupation: this.lookupMappings.yearsCurrentOccupation[this.editUser.yearsCurrentOccupation] || "Unknown"
        };

        this.showEditModal = false;
    } catch (error) {
        console.error("❌ Error updating user details:", error.response?.data || error);
        alert("Failed to update profile. " + (error.response?.data?.message || ""));
    }
}
    }
};</script> 

<style scoped>
    /* 🔹 Profile Container */
    .profile-container {
        padding: 20px;
        max-width: 900px;
        margin: 40px auto;
        background: white;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        text-align: center;
    }

        .profile-container h2 {
            color: #2c3e50;
            font-size: 1.8rem;
            margin-bottom: 20px;
        }

    /* 🔹 Edit Button */
    .edit-btn {
        background-color: #007bff;
        color: white;
        padding: 10px 16px;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-size: 1rem;
        transition: 0.3s;
        margin-bottom: 20px;
    }

        .edit-btn:hover {
            background-color: #0056b3;
        }

    /* 🔹 Profile Card */
    .profile-card {
        background: #ffffff;
        border-radius: 10px;
        padding: 20px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    /* 🔹 Modern Table */
    .modern-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 10px;
        background: white;
        border-radius: 8px;
        overflow: hidden;
    }

        .modern-table th,
        .modern-table td {
            padding: 14px;
            border-bottom: 1px solid #e0e0e0;
            text-align: left;
        }

        .modern-table th {
            background: #f8f9fa;
            font-weight: bold;
            color: #333;
        }

        .modern-table td {
            color: #555;
        }

        /* Alternate row styling */
        .modern-table tr:nth-child(even) {
            background-color: #f9f9f9;
        }

        /* Hover effect */
        .modern-table tr:hover {
            background-color: #f1f1f1;
            transition: 0.3s;
        }

    /* 🔹 Modal Styles */
    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    .modal {
        background: white;
        padding: 30px;
        border-radius: 12px;
        width: 60%;
        max-width: 900px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
    }

        .modal h3 {
            text-align: center;
            color: #2c3e50;
            font-size: 1.6rem;
            margin-bottom: 20px;
        }

    /* 🔹 Form Layout */
    .form-container {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 15px;
    }

    /* 🔹 Form Fields */
    .form-group {
        display: flex;
        flex-direction: column;
    }

        .form-group label {
            font-weight: bold;
            margin-bottom: 5px;
            color: #555;
        }

        .form-group input,
        .form-group select {
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 1rem;
            transition: 0.3s;
        }

            .form-group input:focus,
            .form-group select:focus {
                border-color: #007bff;
                outline: none;
            }

    /* 🔹 Buttons */
    .btn-save {
        background-color: #28a745;
        color: white;
        padding: 12px;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-size: 1rem;
        width: 100%;
        margin-top: 20px;
        transition: 0.3s;
    }

        .btn-save:hover {
            background-color: #218838;
        }

    .btn-cancel {
        background: none;
        color: #007bff;
        border: none;
        cursor: pointer;
        font-size: 1rem;
        text-decoration: underline;
        margin-top: 10px;
    }

        .btn-cancel:hover {
            color: #0056b3;
        }

    /* 🔹 Responsive Design */
    @media (max-width: 768px) {
        .form-container {
            grid-template-columns: 1fr;
        }

        .modal {
            width: 90%;
        }
    }
</style>
