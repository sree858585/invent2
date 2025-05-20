<template>
    <div class="modal-overlay">
        <div class="modal">
            <h2>Add New Training Center</h2>
            <form @submit.prevent="submit">
                <div class="form-grid">
                    <div class="form-group checkbox-group full-width">
                        <label>
                            <input type="checkbox" v-model="isRemoteSite" /> Remote Site
                        </label>
                        <select v-if="isRemoteSite" v-model="form.parentSiteId">
                            <option value="">-- Select Parent Site --</option>
                            <option v-for="site in parentSites" :key="site.siteSysId" :value="site.siteSysId">
                                {{ site.siteName }}
                            </option>
                        </select>
                    </div>

                    <div class="form-group"><label>Training Center Name *</label><input v-model="form.siteName" required /></div>
                    <div class="form-group"><label>Short Name</label><input v-model="form.shortName" /></div>
                    <div class="form-group"><label>Description</label><input v-model="form.description" /></div>
                    <div class="form-group"><label>Address</label><input v-model="form.address" /></div>
                    <div class="form-group"><label>Address 2</label><input v-model="form.address2" /></div>
                    <div class="form-group"><label>City</label><input v-model="form.city" /></div>
                    <div class="form-group">
                        <label>State (2-letter code)</label>
                        <input v-model="form.state" maxlength="2" @input="form.state = form.state.toUpperCase()" placeholder="e.g., NY" />
                    </div>
                    <div class="form-group"><label>Zip</label><input v-model="form.zip" /></div>

                    <div class="form-group"><label>Contact Name</label><input v-model="form.contactName" /></div>
                    <div class="form-group"><label>Contact Email</label><input v-model="form.contactEmail" type="email" /></div>
                    <div class="form-group"><label>Phone</label><input v-model="form.contactPhone" /></div>
                    <div class="form-group"><label>Ext</label><input v-model="form.ext" /></div>
                    <div class="form-group"><label>Website</label><input v-model="form.webUrl" /></div>

                    <div class="form-group">
                        <label>Type</label>
                        <select v-model="form.type">
                            <option value="">-- Select Type --</option>
                            <option v-for="type in contractTypes" :key="type.code" :value="type.code">
                                {{ type.value }}
                            </option>
                        </select>
                    </div>

                    <div class="form-group checkbox-group">
                        <label><input type="checkbox" v-model="form.active" /> Active</label>
                    </div>
                </div>

                <div class="button-group">
                    <button type="submit" class="btn-primary">Submit</button>
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";

    export default {
        emits: ["created", "close"],
        data() {
            return {
                isRemoteSite: false,
                parentSites: [],
                contractTypes: [],
                form: {
                    siteName: "",
                    shortName: "",
                    description: "",
                    address: "",
                    address2: "",
                    city: "",
                    state: "",
                    zip: "",
                    contactName: "",
                    contactEmail: "",
                    contactPhone: "",
                    ext: "",
                    webUrl: "",
                    active: true,
                    type: "",
                    parentSiteId: null,
                },
            };
        },
        async mounted() {
            const [parentsRes, typesRes] = await Promise.all([
                apiClient.get("/TrainingCenter/parent-sites"),
                apiClient.get("/TrainingCenter/contract-types"),
            ]);

            this.parentSites = parentsRes.data?.$values ?? [];
            this.contractTypes = typesRes.data?.$values ?? [];
        },
        methods: {
            async submit() {
                if (!this.isRemoteSite) this.form.parentSiteId = null;
                await apiClient.post("/TrainingCenter/create", this.form);
                alert("Training center created.");
                this.$emit("created");
                this.$emit("close");
            },
        },
    };</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background-color: rgba(0, 0, 0, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 999;
    }

    .modal {
        background-color: white;
        padding: 36px;
        border-radius: 16px;
        width: 680px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 16px 48px rgba(0, 0, 0, 0.2);
        font-family: "Segoe UI", sans-serif;
        animation: fadeIn 0.3s ease;
    }

        .modal h2 {
            font-size: 26px;
            font-weight: 600;
            margin-bottom: 24px;
            text-align: center;
            color: #2c2c2c;
        }

    .form-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
        gap: 20px;
    }

    .form-group {
        display: flex;
        flex-direction: column;
        font-size: 14px;
    }

        .form-group.full-width {
            grid-column: span 2;
        }

        .form-group label {
            margin-bottom: 6px;
            font-weight: 600;
            color: #444;
        }

    input,
    select,
    textarea {
        padding: 12px;
        font-size: 15px;
        border: 1px solid #ccc;
        border-radius: 10px;
        background-color: #f9f9f9;
        transition: all 0.3s ease;
    }

        input:focus,
        select:focus,
        textarea:focus {
            outline: none;
            border-color: #3f51b5;
            background-color: #fff;
            box-shadow: 0 0 0 3px rgba(63, 81, 181, 0.1);
        }

    textarea {
        resize: vertical;
        min-height: 80px;
    }

    .checkbox-group {
        display: flex;
        align-items: center;
        margin-top: 8px;
    }

        .checkbox-group input[type="checkbox"] {
            width: 18px;
            height: 18px;
            margin-right: 10px;
            accent-color: #3f51b5;
            cursor: pointer;
        }

        .checkbox-group label {
            font-weight: 500;
            cursor: pointer;
            user-select: none;
        }

    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 14px;
        margin-top: 30px;
    }

    .btn-primary {
        background-color: #3f51b5;
        color: white;
        padding: 12px 24px;
        border: none;
        font-size: 15px;
        border-radius: 8px;
        font-weight: 600;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(63, 81, 181, 0.2);
        transition: background-color 0.2s ease, box-shadow 0.2s ease;
    }

        .btn-primary:hover {
            background-color: #2f3e94;
            box-shadow: 0 6px 16px rgba(63, 81, 181, 0.3);
        }

    .btn-secondary {
        background-color: #e0e0e0;
        color: #333;
        padding: 12px 24px;
        border: none;
        border-radius: 8px;
        font-size: 15px;
        font-weight: 500;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

        .btn-secondary:hover {
            background-color: #d0d0d0;
        }

    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    @media (max-width: 768px) {
        .modal {
            width: 95%;
            padding: 24px;
        }

        .button-group {
            flex-direction: column;
            align-items: stretch;
        }

        .btn-primary,
        .btn-secondary {
            width: 100%;
        }
    }
</style>
