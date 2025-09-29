<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal-card">
            <header class="modal-head">
                <h2>Create Your Account</h2>
            </header>

            <form class="form-shell" @submit.prevent="handleRegister" novalidate>
                <section class="section-card">
                    <h3 class="section-title">Required Information</h3>

                    <!-- First / Middle Initial / Last -->
                    <div class="grid three-col">
                        <div class="form-group">
                            <label for="firstName" class="label">First Name <span class="req">*</span></label>
                            <input id="firstName" type="text" v-model.trim="form.firstName" placeholder="Jane"
                                   autocomplete="given-name" required @input="touch('firstName')"
                                   :class="{'invalid': isInvalid('firstName')}" />
                            <small v-if="showError('firstName')" class="hint">This field is required.</small>
                        </div>

                        <div class="form-group">
                            <label for="mi" class="label">Middle Initial</label>
                            <input id="mi" type="text" v-model.trim="form.mi" placeholder="M" maxlength="2" />
                        </div>

                        <div class="form-group">
                            <label for="lastName" class="label">Last Name <span class="req">*</span></label>
                            <input id="lastName" type="text" v-model.trim="form.lastName" placeholder="Doe"
                                   autocomplete="family-name" required @input="touch('lastName')"
                                   :class="{'invalid': isInvalid('lastName')}" />
                            <small v-if="showError('lastName')" class="hint">This field is required.</small>
                        </div>
                    </div>

                    <!-- Pronouns -->
                    <div class="grid one-col">
                        <div class="form-group">
                            <label for="pronounId" class="label">Pronouns <span class="req">*</span></label>
                            <select id="pronounId"
                                    v-model.number="form.pronounId"
                                    required
                                    @change="touch('pronounId')"
                                    :class="{'invalid': isInvalid('pronounId')}">
                                <option disabled value="">Select pronouns</option>
                                <option v-for="p in lookups.pronouns" :key="p.pronounId" :value="p.pronounId">
                                    {{ p.label }}
                                </option>
                            </select>
                            <small v-if="showError('pronounId')" class="hint">This field is required.</small>
                        </div>
                    </div>

                    <!-- Email & Confirm -->
                    <div class="grid two-col">
                        <div class="form-group">
                            <label for="email" class="label">Primary Email <span class="req">*</span></label>
                            <input id="email" type="email" v-model.trim="form.email" placeholder="name@example.com"
                                   autocomplete="email" required @input="touch('email')"
                                   :class="{'invalid': isInvalid('email')}" />
                            <small v-if="showError('email')" class="hint">This field is required.</small>
                        </div>

                        <div class="form-group">
                            <label for="confirmEmail" class="label">Confirm Primary Email <span class="req">*</span></label>
                            <input id="confirmEmail" type="email" v-model.trim="form.confirmEmail" placeholder="Re-enter email"
                                   autocomplete="email" required @input="touch('confirmEmail')"
                                   :class="{'invalid': isInvalid('confirmEmail')}" />
                            <small v-if="touched.confirmEmail && !emailsMatch" class="hint">Emails do not match.</small>
                        </div>
                    </div>

                    <!-- Password & Confirm -->
                    <div class="grid two-col">
                        <div class="form-group">
                            <label for="password" class="label">Password <span class="req">*</span></label>
                            <input id="password" type="password" v-model.trim="form.password" placeholder="Create a strong password"
                                   autocomplete="new-password" required @input="touch('password')"
                                   :class="{'invalid': isInvalid('password')}" />
                            <small v-if="showError('password')" class="hint">This field is required.</small>
                        </div>

                        <div class="form-group">
                            <label for="confirmPassword" class="label">Confirm Password <span class="req">*</span></label>
                            <input id="confirmPassword" type="password" v-model.trim="form.confirmPassword" placeholder="Re-enter password"
                                   autocomplete="new-password" required @input="touch('confirmPassword')"
                                   :class="{'invalid': isInvalid('confirmPassword')}" />
                            <small v-if="touched.confirmPassword && !passwordsMatch" class="hint">Passwords do not match.</small>
                        </div>
                    </div>

                    <!-- Recovery Q & A -->
                    <div class="grid two-col">
                        <div class="form-group">
                            <label for="passwordRecoveryQuestion" class="label">Password Recovery Question <span class="req">*</span></label>
                            <select id="passwordRecoveryQuestion" v-model="form.passwordRecoveryQuestion" required
                                    @change="touch('passwordRecoveryQuestion')"
                                    :class="{'invalid': isInvalid('passwordRecoveryQuestion')}">
                                <option disabled value="">Select a question</option>
                                <option>What was the name of your first pet?</option>
                                <option>What is your mother's maiden name?</option>
                                <option>What was your first school?</option>
                            </select>
                            <small v-if="showError('passwordRecoveryQuestion')" class="hint">This field is required.</small>
                        </div>

                        <div class="form-group">
                            <label for="passwordRecoveryAnswer" class="label">Password Recovery Answer <span class="req">*</span></label>
                            <input id="passwordRecoveryAnswer" type="text" v-model.trim="form.passwordRecoveryAnswer"
                                   placeholder="Enter your answer" required @input="touch('passwordRecoveryAnswer')"
                                   :class="{'invalid': isInvalid('passwordRecoveryAnswer')}" />
                            <small v-if="showError('passwordRecoveryAnswer')" class="hint">This field is required.</small>
                        </div>
                    </div>

                    <!-- Phone + can text -->
                    <div class="grid two-col">
                        <div class="form-group">
                            <label for="workPhone" class="label">Phone Number <span class="req">*</span></label>
                            <input id="workPhone" type="tel" inputmode="tel" v-model.trim="form.workPhone" placeholder="(555) 555-1234"
                                   autocomplete="tel" required @input="touch('workPhone')"
                                   :class="{'invalid': isInvalid('workPhone')}" />
                            <small v-if="showError('workPhone')" class="hint">This field is required.</small>
                        </div>

                        <div class="form-group">
                            <label class="label">Can this phone receive texts? <span class="req">*</span></label>
                            <div class="inline-radio">
                                <label><input type="radio" value="true" v-model="form.primaryCanText" @change="touch('primaryCanText')" /> Yes</label>
                                <label><input type="radio" value="false" v-model="form.primaryCanText" @change="touch('primaryCanText')" /> No</label>
                            </div>
                            <small v-if="showError('primaryCanText')" class="hint">Please select Yes or No.</small>
                        </div>
                    </div>
                </section>

                <!-- Optional reveal -->
                <div class="more-link-row">
                    <button type="button" class="link-btn" @click="showMore = !showMore">
                        {{ showMore ? 'Hide additional details' : 'Tell us more about yourself' }}
                        <span class="optional">(optional)</span>
                        <span class="chev">{{ showMore ? '▲' : '▼' }}</span>
                    </button>
                </div>

                <transition name="fold">
                    <section v-if="showMore" class="section-card soft">
                        <h3 class="section-title">Additional Details</h3>

                        <!-- Alt phone + can text -->
                        <div class="grid two-col">
                            <div class="form-group">
                                <label for="altPhone" class="label">Alternate Phone</label>
                                <input id="altPhone" type="tel" inputmode="tel" v-model.trim="form.altPhone" placeholder="(555) 555-5678" />
                            </div>

                            <div class="form-group">
                                <label class="label">Can this alternate phone receive texts?</label>
                                <div class="inline-radio">
                                    <label><input type="radio" value="true" v-model="form.altCanText" /> Yes</label>
                                    <label><input type="radio" value="false" v-model="form.altCanText" /> No</label>
                                </div>
                            </div>
                        </div>

                        <!-- Work Location (dropdown) -->
                        <div class="grid one-col">
                            <div class="form-group">
                                <label for="workLocationId" class="label">Work Location</label>
                                <select id="workLocationId" v-model.number="form.workLocationId">
                                    <option value="">Select Work Location</option>
                                    <option v-for="loc in lookups.workLocations" :key="loc.workLocationId" :value="loc.workLocationId">
                                        {{ loc.label }}
                                    </option>
                                </select>
                            </div>
                        </div>

                        <!-- Optional dropdowns (race, ethnicity, work setting, occupation) -->
                        <div class="grid two-col">
                            <div class="form-group">
                                <label for="workSetting" class="label">Work Setting</label>
                                <select id="workSetting" v-model.number="form.workSetting">
                                    <option :value="null">Select Work Setting</option>
                                    <option v-for="ws in lookups.workSettings" :key="ws.code" :value="ws.code">
                                        {{ ws.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label for="race" class="label">Race</label>
                                <select id="race" v-model.number="form.race">
                                    <option :value="null">Select Race</option>
                                    <option v-for="r in lookups.races" :key="r.code" :value="r.code">{{ r.value }}</option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label for="ethnicity" class="label">Ethnicity</label>
                                <select id="ethnicity" v-model.number="form.ethnicity">
                                    <option :value="null">Select Ethnicity</option>
                                    <option v-for="e in lookups.ethnicities" :key="e.code" :value="e.code">{{ e.value }}</option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label for="occupation" class="label">Occupation</label>
                                <select id="occupation" v-model.number="form.occupation">
                                    <option :value="null">Select Occupation</option>
                                    <option v-for="o in lookups.occupations" :key="o.code" :value="o.code">{{ o.value }}</option>
                                </select>
                            </div>
                        </div>
                    </section>
                </transition>

                <!-- Actions -->
                <footer class="actions">
                    <button type="submit" class="btn-primary" :disabled="!canSubmit">Create Account</button>
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                </footer>
            </form>
        </div>
    </div>
</template>
<script>import apiClient from '@/axios.js';

    export default {
        name: 'RegistrationModal',

        data() {
            return {
                showMore: false,
                lookups: {
                    pronouns: [],
                    workLocations: [],
                    workSettings: [],
                    races: [],
                    ethnicities: [],
                    occupations: []
                },
                form: {
                    // Required
                    firstName: '',
                    mi: '',
                    lastName: '',
                    pronounId: null,   // numeric
                    email: '',
                    confirmEmail: '',
                    password: '',
                    confirmPassword: '',
                    passwordRecoveryQuestion: '',
                    passwordRecoveryAnswer: '',
                    workPhone: '',
                    primaryCanText: '', // "true" | "false"

                    // Optional
                    altPhone: '',
                    altCanText: '',     // "true" | "false"
                    workLocationId: null,
                    workSetting: null,
                    race: null,
                    ethnicity: null,
                    occupation: null
                },
                touched: {
                    firstName: false, lastName: false, email: false, confirmEmail: false,
                    password: false, confirmPassword: false,
                    passwordRecoveryQuestion: false, passwordRecoveryAnswer: false,
                    workPhone: false, pronounId: false, primaryCanText: false
                }
            };
        },

        computed: {
            emailsMatch() {
                return !this.form.confirmEmail || this.form.email === this.form.confirmEmail;
            },
            passwordsMatch() {
                return !this.form.confirmPassword || this.form.password === this.form.confirmPassword;
            },
            canSubmit() {
                const req =
                    this.form.firstName &&
                    this.form.lastName &&
                    this.form.pronounId !== null &&
                    this.form.email &&
                    this.form.confirmEmail &&
                    this.form.password &&
                    this.form.confirmPassword &&
                    this.form.passwordRecoveryQuestion &&
                    this.form.passwordRecoveryAnswer &&
                    this.form.workPhone &&
                    (this.form.primaryCanText === 'true' || this.form.primaryCanText === 'false');
                return req && this.emailsMatch && this.passwordsMatch;
            }
        },

        methods: {
            touch(f) { this.touched[f] = true; },

            isInvalid(f) {
                if (!this.touched[f]) return false;
                if (f === 'confirmEmail') return !this.emailsMatch || !this.form.confirmEmail;
                if (f === 'confirmPassword') return !this.passwordsMatch || !this.form.confirmPassword;
                if (f === 'primaryCanText') return !(this.form.primaryCanText === 'true' || this.form.primaryCanText === 'false');
                return !this.form[f];
            },

            unwrap(list) {
                if (Array.isArray(list)) return list;
                if (list && Array.isArray(list.$values)) return list.$values;
                return [];
            },

            showError(f) { return this.isInvalid(f); },

            async handleRegister() {
                if (!this.canSubmit) {
                    // avoid eslint "unused var" by using for..in
                    for (const key in this.touched) {
                        if (Object.prototype.hasOwnProperty.call(this.touched, key)) {
                            this.touched[key] = true;
                        }
                    }
                    return;
                }
                const payload = {
                    ...this.form,
                    primaryCanText: this.form.primaryCanText === 'true',
                    altCanText: this.form.altCanText === 'true'
                };
                try {
                    await apiClient.post('/registration/register', payload);
                    alert('Registration Successful!');
                    this.$emit('close');
                } catch (err) {
                    console.error(err);
                    alert('Registration Failed. Please try again.');
                }
            },

            // ---- Normalizer: always => { code: number, value: string }
            normalizePairs(list) {
                const arr = Array.isArray(list?.$values) ? list.$values : (list || []);
                const toLabel = (v) => {
                    if (v == null) return '';
                    if (typeof v === 'string' || typeof v === 'number' || typeof v === 'boolean') return String(v);
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

                return arr.map((x) => {
                    if (x && typeof x === 'object' && 'code' in x && 'value' in x)
                        return { code: Number(x.code), value: toLabel(x.value) };
                    if (x && typeof x === 'object' && 'Item1' in x && 'Item2' in x)
                        return { code: Number(x.Item1), value: toLabel(x.Item2) }; // C# ValueTuple
                    if (Array.isArray(x) && x.length >= 2)
                        return { code: Number(x[0]), value: toLabel(x[1]) };
                    return { code: 0, value: toLabel(x) };
                }).filter(x => Number.isFinite(x.code) && x.code !== 0 && x.value !== '');
            },

            async fetchDropdownData() {
                try {
                    const { data } = await apiClient.get('/registration/lookups');
                    console.log('[lookups] RAW keys:', Object.keys(data));

                    // explicit arrays
                    this.lookups.pronouns = this.unwrap(data.Pronouns ?? data.pronouns);
                    this.lookups.workLocations = this.unwrap(data.WorkLocations ?? data.workLocations);

                    // generics
                    this.lookups.workSettings = this.normalizePairs(data.WorkSettings ?? data.workSettings);
                    this.lookups.races = this.normalizePairs(data.Races ?? data.races);
                    this.lookups.ethnicities = this.normalizePairs(data.Ethnicities ?? data.ethnicities);

                    // --- occupations: accept any shape ---
                    const occRaw = data.Occupations ?? data.occupations ?? data.LkOccupations;
                    const occList = this.unwrap(occRaw);
                    console.log('[lookups] occupations RAW sample:', occList[0]);

                    if (!occList.length) {
                        console.warn('[lookups] occupations: empty array from API');
                        this.lookups.occupations = [];
                    } else if (
                        ('code' in (occList[0] || {})) || ('Item1' in (occList[0] || {})) || Array.isArray(occList[0])
                    ) {
                        // already pairs/tuples/arrays
                        this.lookups.occupations = this.normalizePairs(occList);
                    } else if (
                        ('occupationId' in (occList[0] || {})) || ('id' in (occList[0] || {}))
                    ) {
                        // objects like { occupationId, label } or { id, label/name/value/... }
                        this.lookups.occupations = occList.map(o => ({
                            code: Number(o.occupationId ?? o.id),
                            value: String(o.label ?? o.name ?? o.value ?? o.description ?? o.Description ?? '')
                        })).filter(x => Number.isFinite(x.code) && x.code !== 0 && x.value !== '');
                    } else {
                        // last-ditch: try to pull first numeric & first string props
                        this.lookups.occupations = occList.map(o => {
                            if (!o || typeof o !== 'object') return null;
                            const entries = Object.entries(o);
                            const num = entries.find(([, v]) => typeof v === 'number')?.[1];
                            const str = entries.find(([, v]) => typeof v === 'string')?.[1];
                            return (num && str) ? { code: Number(num), value: String(str) } : null;
                        }).filter(Boolean);
                        console.warn('[lookups] occupations fallback mapped. first item:', this.lookups.occupations[0]);
                    }

                    // logs
                    console.log('[lookups] pronouns len:', this.lookups.pronouns.length);
                    console.log('[lookups] workLocations len:', this.lookups.workLocations.length);
                    console.log('[lookups] workSettings len:', this.lookups.workSettings.length);
                    console.log('[lookups] races len:', this.lookups.races.length);
                    console.log('[lookups] ethnicities len:', this.lookups.ethnicities.length);
                    console.log('[lookups] occupations len:', this.lookups.occupations.length);
                } catch (e) {
                    console.error('Error fetching dropdown data:', e);
                }
            }
        },

        mounted() {
            console.log('[RegistrationModal] mounted → fetching dropdown data…');
            this.fetchDropdownData();
        }
    };</script>

<style scoped>
    /* Overlay & Card */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: radial-gradient(ellipse at center,rgba(0,0,0,.72),rgba(0,0,0,.84));
        display: grid;
        place-items: center;
        z-index: 1000;
        padding: 16px
    }

    .modal-card {
        width: min(960px,96vw);
        max-height: 92vh;
        overflow: auto;
        border-radius: 18px;
        background: #fff;
        box-shadow: 0 24px 64px rgba(0,0,0,.35);
        border: 1px solid #eef0f3
    }

    /* Header */
    .modal-head {
        padding: 18px 22px;
        border-bottom: 1px solid #eef0f3;
        background: linear-gradient(180deg,#faf8ff,#f4edff)
    }

        .modal-head h2 {
            margin: 0;
            font-size: 26px;
            font-weight: 700;
            color: #2d1954;
            letter-spacing: .2px
        }

    /* Form & Sections */
    .form-shell {
        padding: 22px;
        display: grid;
        gap: 18px
    }

    .section-card {
        background: #fff;
        border: 1px solid #ecedf0;
        border-radius: 14px;
        padding: 16px 16px 12px;
        box-shadow: 0 2px 14px rgba(33,40,50,.04)
    }

        .section-card.soft {
            background: #fcfdff
        }

    .section-title {
        margin: 0 0 8px;
        font-size: 15px;
        font-weight: 800;
        color: #4a3a77;
        text-transform: uppercase;
        letter-spacing: .5px
    }

    /* Grid */
    .grid {
        display: grid;
        gap: 16px 28px
    }

        .grid.one-col {
            grid-template-columns: 1fr
        }

        .grid.two-col {
            grid-template-columns: repeat(2,minmax(240px,1fr))
        }

        .grid.three-col {
            grid-template-columns: repeat(3,minmax(160px,1fr))
        }

    @media (max-width:900px) {
        .grid.three-col {
            grid-template-columns: 1fr
        }
    }

    @media (max-width:780px) {
        .grid.two-col {
            grid-template-columns: 1fr
        }
    }

    /* Fields */
    .form-group {
        display: grid;
        gap: 6px
    }

    .label {
        font-size: 13px;
        color: #5d5a6f;
        font-weight: 600
    }

    .req {
        color: #b00020
    }

    /* Inputs */
    input, select {
        width: 100%;
        border: 1px solid #d9d3eb;
        background: #fff;
        border-radius: 10px;
        padding: 12px 12px;
        font-size: 15px;
        color: #2a254b;
        transition: border-color .15s,box-shadow .15s,background-color .15s;
        outline: none
    }

        input::placeholder {
            color: #a39dbf
        }

        input:focus, select:focus {
            border-color: #0b3a82;
            box-shadow: 0 0 0 3px rgba(11,58,130,.2);
            background: #fff
        }

    /* Inline radio */
    .inline-radio {
        display: flex;
        gap: 16px;
        align-items: center
    }

        .inline-radio label {
            display: flex;
            gap: 8px;
            align-items: center;
            font-size: 14px;
            color: #2a254b
        }

    /* Validation */
    .invalid {
        border-color: #e11d48 !important;
        box-shadow: 0 0 0 3px rgba(225,29,72,.12) !important
    }

    .hint {
        font-size: 12px;
        color: #b91c1c
    }

    /* More link */
    .more-link-row {
        display: flex;
        justify-content: center;
        margin-top: -6px
    }

    .link-btn {
        background: transparent;
        border: none;
        color: #0b3a82;
        font-weight: 700;
        cursor: pointer;
        text-decoration: underline;
        font-size: 15px;
        display: inline-flex;
        align-items: center;
        gap: 8px
    }

        .link-btn:hover {
            color: #092f6a
        }

    .optional {
        color: #7c7b86;
        font-weight: 600;
        font-size: 13px
    }

    .chev {
        font-size: 12px
    }

    /* Actions */
    .actions {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 12px;
        margin-top: 6px
    }

    .btn-primary, .btn-secondary {
        padding: 12px 16px;
        border-radius: 10px;
        border: none;
        cursor: pointer;
        font-weight: 800;
        font-size: 15px;
        transition: transform .05s,box-shadow .15s,background-color .15s
    }

    .btn-primary {
        background: linear-gradient(180deg,#0b3a82,#092f6a);
        color: #fff;
        border: 1px solid #092f6a;
        box-shadow: 0 6px 14px rgba(11,58,130,.25)
    }

        .btn-primary:hover {
            background: linear-gradient(180deg,#092f6a,#072653);
            border-color: #072653
        }

        .btn-primary:disabled {
            opacity: .7;
            cursor: not-allowed;
            box-shadow: none
        }

    .btn-secondary {
        background: #f4f7fc;
        color: #0b3a82;
        border: 1px solid #e2e8f0
    }

        .btn-secondary:hover {
            background: #eaf0fb;
            border-color: #d8e2f6
        }

    /* Fold transition */
    .fold-enter-active, .fold-leave-active {
        transition: all .18s ease
    }

    .fold-enter-from, .fold-leave-to {
        opacity: 0;
        transform: translateY(-4px)
    }
</style>