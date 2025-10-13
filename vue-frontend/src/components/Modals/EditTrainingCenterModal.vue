<template>
    <div class="modal-overlay" v-if="form">
        <div class="modal" role="dialog" aria-modal="true">
            <button class="icon-close danger modal-close" @click="$emit('close')" aria-label="Close">✖</button>

            <h2>Edit Training Center</h2>

            <form @submit.prevent="submit">
                <div class="form-grid">
                    <!-- Remote Site -->
                    <div class="form-group checkbox-group full-width">
                        <label>
                            <input type="checkbox" v-model="isRemoteSite" /> Remote Site
                        </label>
                        <select v-if="isRemoteSite" v-model.number="form.parentSiteId">
                            <option :value="null">-- Select Parent Site --</option>
                            <option v-for="site in parentSites" :key="site.siteSysId" :value="site.siteSysId">
                                {{ site.siteName }}
                            </option>
                        </select>
                    </div>

                    <!-- Basics -->
                    <div class="form-group"><label>Training Center Name *</label><input v-model="form.siteName" required /></div>
                    <div class="form-group"><label>Short Name</label><input v-model="form.shortName" /></div>
                    <!-- (Description removed from here) -->
                    <div class="form-group"><label>Address</label><input v-model="form.address" /></div>
                    <div class="form-group"><label>Address 2</label><input v-model="form.address2" /></div>
                    <div class="form-group"><label>City</label><input v-model="form.city" /></div>
                    <div class="form-group">
                        <label>State (2-letter code)</label>
                        <input v-model="form.state" maxlength="2" @input="form.state = (form.state || '').toUpperCase()" placeholder="e.g., NY" />
                    </div>
                    <div class="form-group"><label>Zip</label><input v-model="form.zip" /></div>

                    <!-- Region -->
                    <div class="form-group">
                        <label>Region</label>
                        <select v-model.number="form.regionCode">
                            <option :value="null">-- Select Region --</option>
                            <option v-for="r in regions" :key="r.code" :value="r.code">
                                {{ r.name }}
                            </option>
                        </select>
                    </div>

                    <!-- Contact header -->
                    <div class="form-group full-width">
                        <h4 class="section-header">Contact Info</h4>
                    </div>

                    <!-- Contact fields -->
                    <div class="form-group"><label>Contact Name</label><input v-model="form.contactName" /></div>
                    <div class="form-group"><label>Email</label><input v-model="form.contactEmail" type="email" /></div>
                    <div class="form-group"><label>Phone</label><input v-model="form.contactPhone" /></div>
                    <div class="form-group"><label>Ext</label><input v-model="form.ext" /></div>
                    <div class="form-group"><label>Website</label><input v-model="form.webUrl" /></div>

                    <!-- Type / Active -->
                    <div class="form-group">
                        <label>Type</label>
                        <select v-model.number="form.type">
                            <option :value="null">-- Select Type --</option>
                            <option v-for="type in contractTypes" :key="type.code" :value="type.code">
                                {{ type.value }}
                            </option>
                        </select>
                    </div>

                    <div class="form-group checkbox-group">
                        <label><input type="checkbox" v-model="form.active" /> Active</label>
                    </div>

                    <!-- Description moved to end -->
                    <div class="form-group full-width">
                        <label>Description</label>
                        <textarea v-model="form.description" rows="3" placeholder="Optional description…"></textarea>
                    </div>
                </div>

                <div class="button-group">
                    <button type="submit" class="btn-primary">Update</button>
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";

    export default {
        props: { center: Object },
        emits: ["close", "updated"],
        data() {
            return {
                form: null,
                parentSites: [],
                contractTypes: [],
                regions: [],        // NEW
                isRemoteSite: false,
            };
        },
        async mounted() {
            const [parentsRes, typesRes, regionsRes] = await Promise.all([
                apiClient.get("/TrainingCenter/parent-sites"),
                apiClient.get("/TrainingCenter/contract-types"),
                apiClient.get("/TrainingCenter/regions"),
            ]);
            this.parentSites = parentsRes.data?.$values ?? parentsRes.data ?? [];
            this.contractTypes = typesRes.data?.$values ?? typesRes.data ?? [];
            this.regions = regionsRes.data?.$values ?? regionsRes.data ?? [];

            const { data } = await apiClient.get(`/TrainingCenter/${this.center.siteSysId}`);
            this.form = {
                ...data,
                parentSiteId: data.parentSiteId ?? null,
                type: data.type ?? null,
                regionCode: data.regionCode ?? null,  // NEW
            };
            this.isRemoteSite = !!this.form.parentSiteId;
            this._esc = (e) => { if (e.key === 'Escape') this.$emit('close'); };
            window.addEventListener('keydown', this._esc);
        },
        beforeUnmount() {
            window.removeEventListener('keydown', this._esc);
        },
        methods: {
            async submit() {
                if (!this.isRemoteSite) this.form.parentSiteId = null;
                await apiClient.put(`/TrainingCenter/update/${this.form.siteSysId}`, this.form);
                alert("✅ Training center updated.");
                this.$emit("updated");
                this.$emit("close");
            },
        },
    };</script>

<style scoped>
    /* exactly your previous styles, unchanged */
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
        width: 800px;
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

    input, select, textarea {
        padding: 12px;
        font-size: 15px;
        border: 1px solid #ccc;
        border-radius: 10px;
        background-color: #f9f9f9;
        transition: all 0.3s ease;
    }

        input:focus, select:focus, textarea:focus {
            outline: none;
            border-color: #3f51b5;
            background-color: #fff;
            box-shadow: 0 0 0 3px rgba(63,81,181,0.1);
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
        box-shadow: 0 4px 12px rgba(63,81,181,0.2);
        transition: background-color 0.2s ease, box-shadow 0.2s ease;
    }

        .btn-primary:hover {
            background-color: #2f3e94;
            box-shadow: 0 6px 16px rgba(63,81,181,0.3);
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

        .btn-primary, .btn-secondary {
            width: 100%;
        }
    }
    .modal {
        position: relative;
    }
    /* allows absolute button positioning */

    .icon-close {
        border: none;
        background: #f5f5f5;
        border-radius: 8px;
        padding: 6px 10px;
        cursor: pointer;
        line-height: 1;
        font-size: 16px;
    }

        .icon-close.danger {
            background: #ffe7e7;
            color: #b71c1c;
            border: 1px solid #ffc9c9;
        }

            .icon-close.danger:hover {
                background: #ffd7d7;
            }

    .modal-close {
        position: absolute;
        top: 14px;
        right: 14px;
    }
    .section-header {
        margin: 4px 0 6px;
        font-size: 16px;
        font-weight: 700;
        color: #333;
    }
</style>