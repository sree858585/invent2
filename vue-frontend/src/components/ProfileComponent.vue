<template>
    <div class="profile-container">
        <h2>User Profile</h2>
        <button class="edit-btn" @click="openEditModal">View / Edit Profile</button>

        <!-- Encourage completion (only if something optional is missing) -->
        <div v-if="user && needsMoreInfo" class="complete-banner">
            <div class="banner-left">
                <div class="banner-title">Tell us more about yourself</div>
                <div class="banner-sub">
                    Add a few optional details so we can personalize your experience.
                </div>
            </div>
            <div class="banner-actions">
                <button class="btn-primary btn-sm" @click="openEditModal">Complete profile</button>
            </div>
        </div>

        <div v-if="user" class="profile-card">
            <table class="modern-table">
                <tbody>
                    <tr>
                        <th>Name</th>
                        <td>{{ fullName }}</td>
                    </tr>

                    <tr>
                        <th>Email</th>
                        <td>{{ user.email }}</td>
                    </tr>

                    <tr>
                        <th>Alternate Email</th>
                        <td>{{ user.altEmail || '—' }}</td>
                    </tr>

                    <tr>
                        <th>Pronouns</th>
                        <td>{{ user.pronounLabel || '—' }}</td>
                    </tr>

                    <tr>
                        <th>Work Location</th>
                        <td>{{ user.workLocationLabel || '—' }}</td>
                    </tr>

                    <tr>
                        <th>Work Setting</th>
                        <td>{{ user.workSettingLabel || '—' }}</td>
                    </tr>

                    <tr>
                        <th>Ethnicity</th>
                        <td>{{ user.ethnicityLabel || '—' }}</td>
                    </tr>

                    <tr>
                        <th>Race</th>
                        <td>{{ user.raceLabel || '—' }}</td>
                    </tr>

                    <tr>
                        <th>Occupation</th>
                        <td>{{ user.occupationLabel || '—' }}</td>
                    </tr>

                    <!-- Phones shown with "Can text?" next to them -->
                    <tr>
                        <th>Work Phone</th>
                        <td class="phone-row">
                            <span class="phone">{{ user.workPhone || '—' }}</span>
                            <span class="can-text">Can text? <strong>{{ yesNo(user.primaryCanText) }}</strong></span>
                        </td>
                    </tr>

                    <tr>
                        <th>Alternate Phone</th>
                        <td class="phone-row">
                            <span class="phone">{{ user.cellPhone || '—' }}</span>
                            <span class="can-text">Can text? <strong>{{ yesNo(user.altCanText) }}</strong></span>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <p v-else>Loading user details...</p>

        <!-- Edit Modal -->
        <div v-if="showEditModal" class="modal-overlay" @click.self="showEditModal=false">
            <div class="modal modal-wide">

                <!-- Header -->
                <header class="modal-header">
                    <h3 class="modal-title">Edit Profile</h3>
                    <button class="icon-close danger" @click="showEditModal=false" aria-label="Close">✖</button>
                </header>

                <!-- Small, centered Save under title -->
                <div class="actions-top">
                    <button type="button" class="btn-primary btn-sm" @click="updateUserProfile">Save Changes</button>
                </div>

                <form @submit.prevent="updateUserProfile">
                    <div class="form-grid">
                        <!-- Names & Email -->
                        <div class="form-group">
                            <label>First Name</label>
                            <input type="text" v-model.trim="editUser.firstName" required />
                        </div>

                        <div class="form-group">
                            <label>Middle Initial</label>
                            <input type="text" v-model.trim="editUser.mi" maxlength="2" />
                        </div>

                        <div class="form-group">
                            <label>Last Name</label>
                            <input type="text" v-model.trim="editUser.lastName" required />
                        </div>

                        <div class="form-group">
                            <label>Pronouns</label>
                            <select v-model.number="editUser.pronounId" required>
                                <option :value="null" disabled>Select pronouns</option>
                                <option v-for="p in lookups.pronouns" :key="p.pronounId" :value="p.pronounId">
                                    {{ p.label }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Email</label>
                            <input type="email" v-model.trim="editUser.email" required />
                        </div>
                        <div class="form-group">
                            <label>Alternate Email</label>
                            <input type="email" v-model.trim="editUser.altEmail" />
                        </div>
                        <div class="form-group">
                            <label>Work Location</label>
                            <select v-model.number="editUser.workLocationId">
                                <option :value="null">Select Work Location</option>
                                <option v-for="w in lookups.workLocations" :key="w.workLocationId" :value="w.workLocationId">
                                    {{ w.label }}
                                </option>
                            </select>
                        </div>

                        <!-- Lookups -->
                        <div class="form-group">
                            <label>Work Setting</label>
                            <select v-model.number="editUser.workSetting">
                                <option :value="null">Select Work Setting</option>
                                <option v-for="(label, code) in lookupMaps.workSettings" :key="code" :value="toNum(code)">
                                    {{ label }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Ethnicity</label>
                            <select v-model.number="editUser.ethnicity">
                                <option :value="null">Select Ethnicity</option>
                                <option v-for="(label, code) in lookupMaps.ethnicities" :key="code" :value="toNum(code)">
                                    {{ label }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Race</label>
                            <select v-model.number="editUser.race">
                                <option :value="null">Select Race</option>
                                <option v-for="(label, code) in lookupMaps.races" :key="code" :value="toNum(code)">
                                    {{ label }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Occupation</label>
                            <select v-model.number="editUser.occupation">
                                <option :value="null">Select Occupation</option>
                                <option v-for="(label, code) in lookupMaps.occupations" :key="code" :value="toNum(code)">
                                    {{ label }}
                                </option>
                            </select>
                        </div>

                        <!-- Phones -->
                        <div class="form-group">
                            <label>Phone Number</label>
                            <input type="text" v-model.trim="editUser.workPhone" required />
                        </div>

                        <div class="form-group">
                            <label>Can text this phone?</label>
                            <div class="inline-radio">
                                <label><input type="radio" :value="true" v-model="editUser.primaryCanText" /> Yes</label>
                                <label><input type="radio" :value="false" v-model="editUser.primaryCanText" /> No</label>
                            </div>
                        </div>

                        <div class="form-group">
                            <label>Alternate Phone Number</label>
                            <input type="text" v-model.trim="editUser.cellPhone" />
                        </div>

                        <div class="form-group">
                            <label>Can text alternate phone?</label>
                            <div class="inline-radio">
                                <label><input type="radio" :value="true" v-model="editUser.altCanText" /> Yes</label>
                                <label><input type="radio" :value="false" v-model="editUser.altCanText" /> No</label>
                            </div>
                        </div>
                    </div>

                    <!-- Bottom actions -->
                    <div class="actions-bottom">
                        <button type="submit" class="btn-primary">Save Changes</button>
                        <button type="button" class="btn-link" @click="showEditModal=false">Cancel</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios.js';

    export default {
        name: 'ProfileComponent',
        data() {
            return {
                user: null,          // server-shape dto
                editUser: {},        // modal model (codes + booleans)
                showEditModal: false,

                lookups: {           // arrays from /registration/lookups
                    pronouns: [],
                    workLocations: []
                },
                lookupMaps: {        // { code:number -> label:string }
                    workSettings: {},
                    ethnicities: {},
                    races: {},
                    occupations: {}
                }
            };
        },

        computed: {
            fullName() {
                const mi = this.user?.mi ? ` ${this.user.mi} ` : ' ';
                return `${this.user?.firstName || ''}${mi}${this.user?.lastName || ''}`.trim();
            },

            // 🔑 now a real computed prop
            needsMoreInfo() {
                const u = this.user;
                if (!u) return false;

                const isMissing = v => v === null || v === undefined;
                const isBlank = v => v == null || (typeof v === 'string' && v.trim() === '');

                // mark true if ANY optional item is missing
                return [
                    isMissing(u.workLocationId),
                    isMissing(u.workSetting),
                    isMissing(u.ethnicity),
                    isMissing(u.race),
                    isMissing(u.occupation),
                    isBlank(u.cellPhone),
                    isMissing(u.altCanText),
                    isBlank(u.altEmail)           

                ].some(Boolean);
            }
        },
        

        async mounted() {
            const userId = localStorage.getItem('userId');
            if (!userId) {
                alert('User ID not found. Please log in again.');
                return;
            }
            try {
                await this.loadLookups();
                const { data } = await apiClient.get(`/user/${userId}`);
                this.user = data;
            } catch (e) {
                console.error(e);
                alert('Failed to load profile.');
            }
        },

        methods: {
            // ----- helpers -----
            yesNo(v) { return v === true ? 'Yes' : v === false ? 'No' : '—'; },
            toNum(k) { const n = Number(k); return Number.isNaN(n) ? null : n; },

            unwrap(list) {
                if (Array.isArray(list)) return list;
                if (list && Array.isArray(list.$values)) return list.$values;
                return [];
            },
            normalizePairs(list) {
                const arr = Array.isArray(list?.$values) ? list.$values : (list || []);
                const toLabel = (v) => {
                    if (v == null) return '';
                    if (['string', 'number', 'boolean'].includes(typeof v)) return String(v);
                    if (typeof v === 'object') {
                        if ('label' in v) return String(v.label);
                        if ('name' in v) return String(v.name);
                        if ('Value' in v) return String(v.Value);
                        if ('Text' in v) return String(v.Text);
                        if ('value' in v) return String(v.value);
                        if ('Description' in v) return String(v.Description);
                        if ('description' in v) return String(v.description);
                    }
                    return String(v);
                };
                return arr.map(x => {
                    if (x && typeof x === 'object' && 'code' in x && 'value' in x) return { code: Number(x.code), value: toLabel(x.value) };
                    if (x && typeof x === 'object' && 'Item1' in x && 'Item2' in x) return { code: Number(x.Item1), value: toLabel(x.Item2) };
                    if (Array.isArray(x) && x.length >= 2) return { code: Number(x[0]), value: toLabel(x[1]) };
                    return { code: 0, value: toLabel(x) };
                }).filter(x => Number.isFinite(x.code) && x.code !== 0 && x.value !== '');
            },
            toMap(list) {
                const m = {};
                this.normalizePairs(list).forEach(i => { m[i.code] = i.value; });
                return m;
            },

            async loadLookups() {
                const { data } = await apiClient.get('/registration/lookups');

                // arrays with ids
                this.lookups.pronouns = this.unwrap(data.Pronouns ?? data.pronouns);
                this.lookups.workLocations = this.unwrap(data.WorkLocations ?? data.workLocations);

                // code->label maps
                this.lookupMaps.workSettings = this.toMap(data.WorkSettings ?? data.workSettings);
                this.lookupMaps.ethnicities = this.toMap(data.Ethnicities ?? data.ethnicities);
                this.lookupMaps.races = this.toMap(data.Races ?? data.races);
                this.lookupMaps.occupations = this.toMap(data.Occupations ?? data.occupations);
            },

            openEditModal() {
                if (!this.user) return;
                // seed with codes (already provided by server dto)
                this.editUser = {
                    firstName: this.user.firstName || '',
                    mi: this.user.mi || '',
                    lastName: this.user.lastName || '',
                    email: this.user.email || '',
                    altEmail: this.user.altEmail || '',        

                    pronounId: this.user.pronounId ?? null,
                    workLocationId: this.user.workLocationId ?? null,

                    workSetting: this.user.workSetting ?? null,
                    ethnicity: this.user.ethnicity ?? null,
                    race: this.user.race ?? null,
                    occupation: this.user.occupation ?? null,

                    workPhone: this.user.workPhone || '',
                    primaryCanText: this.user.primaryCanText ?? null,

                    cellPhone: this.user.cellPhone || '',
                    altCanText: this.user.altCanText ?? null
                };
                this.showEditModal = true;
            },

            async updateUserProfile() {
                try {
                    const userId = localStorage.getItem('userId');
                    if (!userId) { alert('User ID not found.'); return; }

                    // payload must match UserUpdateDto
                    const payload = {
                        firstName: this.editUser.firstName,
                        mi: this.editUser.mi || null,
                        lastName: this.editUser.lastName,
                        email: this.editUser.email,
                        altEmail: this.editUser.altEmail || null,    

                        pronounId: this.editUser.pronounId ?? null,
                        workLocationId: this.editUser.workLocationId ?? null,
                        workSetting: this.editUser.workSetting ?? null,
                        ethnicity: this.editUser.ethnicity ?? null,
                        race: this.editUser.race ?? null,
                        occupation: this.editUser.occupation ?? null,

                        workPhone: this.editUser.workPhone,
                        primaryCanText: (this.editUser.primaryCanText === true || this.editUser.primaryCanText === false)
                            ? this.editUser.primaryCanText : null,

                        cellPhone: this.editUser.cellPhone || null,
                        altCanText: (this.editUser.altCanText === true || this.editUser.altCanText === false)
                            ? this.editUser.altCanText : null
                    };

                    const { data } = await apiClient.put(`/user/${userId}`, payload);

                    // server returns fresh dto with labels
                    this.user = data;
                    this.showEditModal = false;
                    alert('Profile updated successfully!');
                } catch (e) {
                    console.error(e);
                    alert('Failed to update profile.');
                }
            }
        }
    };</script>
<style scoped>
    /* ========= Profile (page content) ========= */
    .profile-container {
        padding: 20px;
        max-width: 900px;
        margin: 40px auto;
        background: #fff;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0,0,0,.1);
        text-align: center;
        position: relative; /* keeps its own stacking context */
        z-index: 1; /* sits below the modal overlay */
    }

        .profile-container h2 {
            color: #2c3e50;
            font-size: 1.8rem;
            margin-bottom: 20px;
        }

    /* Edit button */
    .edit-btn {
        background: #007bff;
        color: #fff;
        padding: 10px 16px;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-size: 1rem;
        transition: .3s;
        margin-bottom: 20px;
    }

        .edit-btn:hover {
            background: #0056b3
        }

    /* Card + table */
    .profile-card {
        background: #fff;
        border-radius: 10px;
        padding: 20px;
        box-shadow: 0 4px 12px rgba(0,0,0,.1)
    }

    .modern-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 10px;
        background: #fff;
        border-radius: 8px;
        overflow: hidden
    }

        .modern-table th, .modern-table td {
            padding: 14px;
            border-bottom: 1px solid #e0e0e0;
            text-align: left
        }

        .modern-table th {
            background: #f8f9fa;
            font-weight: 700;
            color: #333
        }

        .modern-table td {
            color: #555
        }

        .modern-table tr:nth-child(even) {
            background: #f9f9f9
        }

        .modern-table tr:hover {
            background: #f1f1f1;
            transition: .3s
        }

    /* Phones inline */
    .phone-row {
        display: flex;
        gap: 18px;
        align-items: center
    }

        .phone-row .phone {
            font-weight: 600
        }

        .phone-row .can-text {
            color: #444
        }

    /* ========= Modal (sits ON TOP) ========= */
    .modal-overlay {
        position: fixed; /* covers the whole viewport */
        inset: 0; /* top/right/bottom/left:0 */
        background: rgba(0,0,0,.55);
        backdrop-filter: saturate(120%) blur(2px);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 9999; /* higher than any page content */
    }

    /* Modal panel */
    .modal {
        position: relative;
        background: #fff;
        padding: 24px 28px;
        border-radius: 16px;
        width: min(880px,92vw);
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 24px 64px rgba(0,0,0,.22);
        border: 1px solid #eef0f3;
        z-index: 10000; /* above overlay backdrop */
    }

    /* Header with close */
    .modal-header {
        position: relative;
        padding: 8px 0 6px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .modal-title {
        margin: 0;
        font-size: 22px;
        font-weight: 800;
        letter-spacing: .2px;
        color: #25324b;
    }

    .icon-close.danger {
        position: absolute;
        top: 6px;
        right: 6px;
        background: #e53935;
        color: #fff;
        border: none;
        border-radius: 999px;
        width: 34px;
        height: 34px;
        font-size: 16px;
        line-height: 34px;
        text-align: center;
        cursor: pointer
    }

        .icon-close.danger:hover {
            background: #c62828
        }

    /* Small centered save under title */
    .actions-top {
        display: grid;
        place-items: center;
        margin: 6px 0 18px
    }

    .btn-sm {
        padding: 8px 14px;
        font-size: 13px;
        border-radius: 10px
    }

    /* Buttons */
    .btn-primary {
        background: linear-gradient(180deg,#0b8a4a,#08703b);
        color: #fff;
        border: 1px solid #0a6f3e;
        border-radius: 12px;
        padding: 11px 18px;
        font-weight: 800;
        letter-spacing: .2px;
        cursor: pointer;
        transition: box-shadow .15s,transform .05s,background-color .15s;
    }

        .btn-primary:hover {
            background: linear-gradient(180deg,#08703b,#065a30)
        }

        .btn-primary:active {
            transform: translateY(1px)
        }

    .btn-link {
        background: transparent;
        border: none;
        color: #0b3a82;
        font-weight: 700;
        cursor: pointer;
        text-decoration: underline;
        padding: 10px 12px
    }

    .actions-bottom {
        display: flex;
        gap: 12px;
        justify-content: center;
        margin-top: 18px
    }

    /* Form grid */
    .form-grid {
        display: grid;
        grid-template-columns: repeat(2,minmax(240px,1fr));
        gap: 18px 22px
    }

    @media (max-width:780px) {
        .form-grid {
            grid-template-columns: 1fr
        }
    }

    .form-group {
        display: grid;
        gap: 8px
    }

        .form-group label {
            font-size: 13px;
            font-weight: 800;
            color: #4b5563
        }

        .form-group input, .form-group select {
            width: 100%;
            border: 1px solid #dfe3eb;
            background: #fff;
            border-radius: 12px;
            padding: 12px;
            font-size: 14px;
            color: #1f2937;
            transition: border-color .15s,box-shadow .15s,background-color .15s;
            outline: none
        }

            .form-group input:focus, .form-group select:focus {
                border-color: #0b3a82;
                box-shadow: 0 0 0 3px rgba(11,58,130,.18)
            }

    .inline-radio {
        display: inline-flex;
        gap: 18px;
        align-items: center;
        padding: 10px 0
    }

        .inline-radio label {
            display: inline-flex;
            gap: 8px;
            align-items: center;
            font-size: 14px;
            color: #1f2937
        }
    /* Encourage completion banner */
    .complete-banner {
        margin: 14px 0 18px;
        padding: 16px 18px;
        border: 1px solid #e8ecf4;
        background: linear-gradient(0deg,#f8fbff,#ffffff);
        border-radius: 12px;
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        text-align: left;
    }

    .banner-left {
        display: grid;
        gap: 4px
    }

    .banner-title {
        font-weight: 800;
        color: #1f2a44;
        letter-spacing: .2px;
    }

    .banner-sub {
        color: #51617b;
        font-size: .95rem;
    }

    .banner-actions .btn-sm {
        padding: 8px 14px;
        font-size: 13px;
        border-radius: 10px;
    }

    /* Reuse your primary button style */
</style>