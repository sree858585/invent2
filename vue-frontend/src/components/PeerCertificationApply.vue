<template>
    <div class="apply-wrap">
        <!-- ✅ Login Modal -->
        <LoginModal v-if="showLogin"
                    @close="showLogin = false"
                    @login-success="handleLoginSuccess" />

        <div class="apply-card">
            <!-- Header -->
            <div class="header">
                <h1>Peer Certification Application</h1>
                <p>Complete steps 1–6. You can save and return anytime.</p>
            </div>

            <!-- Step Tracker -->
            <div class="stepper">
                <div v-for="s in steps"
                     :key="s.id"
                     class="step"
                     :class="[currentStep === s.id ? 'active' : '', currentStep > s.id ? 'done' : '']"
                     @click="goToStep(s.id)">
                    <div class="circle">{{ s.id }}</div>
                    <div class="label">{{ s.label }}</div>
                    <div v-if="s.id !== steps.length" class="line"></div>
                </div>
            </div>

            <!-- Body -->
            <div class="body">
                <!-- Step 1 -->
                <div v-if="currentStep === 1">
                    <h3>Step 1: Applicant Info</h3>

                    <div v-if="loading" class="muted">Loading...</div>

                    <div v-else class="form-grid">
                        <div class="field">
                            <label>First Name *</label>
                            <input v-model.trim="form.FirstName"
                                   type="text"
                                   :disabled="locked.FirstName"
                                   :class="{ error: errors.FirstName }" />
                            <div v-if="errors.FirstName" class="error-text">{{ errors.FirstName }}</div>
                        </div>

                        <div class="field">
                            <label>Last Name *</label>
                            <input v-model.trim="form.LastName"
                                   type="text"
                                   :disabled="locked.LastName"
                                   :class="{ error: errors.LastName }" />
                            <div v-if="errors.LastName" class="error-text">{{ errors.LastName }}</div>
                        </div>

                        <div class="field span-2">
                            <label>Address *</label>
                            <input v-model.trim="form.Address"
                                   type="text"
                                   :disabled="locked.Address"
                                   :class="{ error: errors.Address }" />
                            <div v-if="errors.Address" class="error-text">{{ errors.Address }}</div>
                        </div>

                        <div class="field">
                            <label>City *</label>
                            <input v-model.trim="form.City"
                                   type="text"
                                   :disabled="locked.City"
                                   :class="{ error: errors.City }" />
                            <div v-if="errors.City" class="error-text">{{ errors.City }}</div>
                        </div>

                        <div class="field">
                            <label>State *</label>
                            <input v-model.trim="form.State"
                                   type="text"
                                   :disabled="locked.State"
                                   :class="{ error: errors.State }" />
                            <div v-if="errors.State" class="error-text">{{ errors.State }}</div>
                        </div>

                        <div class="field">
                            <label>Zip *</label>
                            <input v-model.trim="form.Zip"
                                   type="text"
                                   :disabled="locked.Zip"
                                   :class="{ error: errors.Zip }" />
                            <div v-if="errors.Zip" class="error-text">{{ errors.Zip }}</div>
                        </div>

                        <div class="span-2 phone-row">
                            <div class="field">
                                <label>Work Phone (optional)</label>
                                <input v-model.trim="form.WorkPhone" type="text" :disabled="locked.WorkPhone" />
                            </div>

                            <div class="field">
                                <label>Ext (optional)</label>
                                <input v-model.trim="form.WorkPhoneExt" type="text" :disabled="locked.WorkPhoneExt" />
                            </div>

                            <div class="field cell-line">
                                <label>Cell Phone (optional)</label>
                                <input v-model.trim="form.CellPhone" type="text" :disabled="locked.CellPhone" />
                            </div>
                        </div>

                        <div class="field">
                            <label>Ethnicity *</label>
                            <!-- ✅ editable even if value exists -->
                            <select v-model.number="form.Ethnicity"
                                    :disabled="false"
                                    :class="{ error: errors.Ethnicity }">
                                <option :value="null">-- Select --</option>
                                <option v-for="e in lookups.ethnicities" :key="e.code" :value="e.code">
                                    {{ e.value }}
                                </option>
                            </select>
                            <div v-if="errors.Ethnicity" class="error-text">{{ errors.Ethnicity }}</div>
                        </div>

                        <div class="field">
                            <label>Race *</label>
                            <!-- ✅ editable even if value exists -->
                            <select v-model.number="form.Race"
                                    :disabled="false"
                                    :class="{ error: errors.Race }">
                                <option :value="null">-- Select --</option>
                                <option v-for="r in lookups.races" :key="r.code" :value="r.code">
                                    {{ r.value }}
                                </option>
                            </select>
                            <div v-if="errors.Race" class="error-text">{{ errors.Race }}</div>
                        </div>

                        <div class="field">
                            <label>Education *</label>
                            <!-- ✅ editable even if value exists -->
                            <select v-model.number="form.Education"
                                    :disabled="false"
                                    :class="{ error: errors.Education }">
                                <option :value="null">-- Select --</option>
                                <option v-for="ed in lookups.educations" :key="ed.code" :value="ed.code">
                                    {{ ed.value }}
                                </option>
                            </select>
                            <div v-if="errors.Education" class="error-text">{{ errors.Education }}</div>
                        </div>

                        <div class="field">
                            <label>Title *</label>
                            <input v-model.trim="form.Title"
                                   type="text"
                                   :disabled="locked.Title"
                                   :class="{ error: errors.Title }" />
                            <div v-if="errors.Title" class="error-text">{{ errors.Title }}</div>
                        </div>

                        <div class="field span-2">
                            <label>Certification Track *</label>

                            <div class="track-multi" :class="{ error: errors.CertificationTrack }">
                                <label class="track-option" :class="{ disabled: isHivDisabled }">
                                    <input type="checkbox"
                                           value="HIV"
                                           v-model="form.CertificationTrack"
                                           :disabled="isHivDisabled && !form.CertificationTrack.includes('HIV')" />
                                    <span>HIV Peer Worker</span>
                                </label>

                                <label class="track-option">
                                    <input type="checkbox" value="HCV" v-model="form.CertificationTrack" />
                                    <span>HCV Peer Worker</span>
                                </label>

                                <label class="track-option">
                                    <input type="checkbox" value="HR" v-model="form.CertificationTrack" />
                                    <span>Harm Reduction Peer Worker</span>
                                </label>

                                <label class="track-option" :class="{ disabled: isPrepDisabled }">
                                    <input type="checkbox"
                                           value="PREP"
                                           v-model="form.CertificationTrack"
                                           :disabled="isPrepDisabled && !form.CertificationTrack.includes('PREP')" />
                                    <span>PrEP Peer Worker</span>
                                </label>

                                <label class="track-option">
                                    <input type="checkbox" value="CJ" v-model="form.CertificationTrack" />
                                    <span>Criminal Justice Peer Worker</span>
                                </label>
                            </div>

                            <div v-if="errors.CertificationTrack" class="error-text">
                                {{ errors.CertificationTrack }}
                            </div>
                            <div v-if="trackConflictMessage" class="error-text">
                                {{ trackConflictMessage }}
                            </div>
                        </div>

                        <div class="field">
                            <label>Date of Birth *</label>
                            <input v-model="form.Dob"
                                   type="date"
                                   :disabled="locked.Dob"
                                   :class="{ error: errors.Dob }" />
                            <div v-if="errors.Dob" class="error-text">{{ errors.Dob }}</div>
                        </div>

                        <div class="field">
                            <label>Gender *</label>
                            <select v-model.number="form.Gender"
                                    :disabled="locked.Gender"
                                    :class="{ error: errors.Gender }">
                                <option :value="null">-- Select --</option>
                                <option v-for="g in lookups.genders" :key="g.code" :value="g.code">
                                    {{ g.value }}
                                </option>
                            </select>
                            <div v-if="errors.Gender" class="error-text">{{ errors.Gender }}</div>
                        </div>

                        <div class="span-2 muted" v-if="saveMessage">{{ saveMessage }}</div>
                    </div>
                </div>
                <div v-else-if="currentStep === 2">
                    <h3 class="le-title">Step 2: Lived Experience</h3>

                    <!-- optional summary banner -->
                    <div v-if="step2HasErrors" class="le-alert" role="alert" aria-live="polite">
                        Please complete all three required responses. Each response must be at least 500 characters.
                    </div>

                    <div class="le-card">
                        <p class="le-text">
                            As of May 26, 2017, all peer workers applying for AI Certification are required to submit
                            responses to three questions related to their lived experience of HIV, HCV and/or Harm
                            Reduction, regardless of whether they completed Foundational Training.
                        </p>

                        <p class="le-text">
                            <strong>Questions:</strong><br />
                            The following questions allow our AI Peer Worker Certification Review Board to gauge your
                            personal preparedness for peer work in lieu of completing Foundational Trainings. In order for
                            your responses to be saved, you need to answer <strong>ALL THREE QUESTIONS</strong>. Each
                            question is required to have a 500 character response.
                        </p>

                        <p class="le-note"><strong>All answers must be at least 500 characters.</strong></p>
                    </div>

                    <div class="le-grid">
                        <!-- Q1 -->
                        <div class="le-q">
                            <label class="le-label" for="expCommitment">
                                <span class="req">*</span>
                                Describe your personal commitment to wellness as it relates to your lived experience of HIV, HCV or harm reduction:
                            </label>

                            <textarea id="expCommitment"
                                      class="le-textarea"
                                      v-model.trim="form.ExperienceCommitment"
                                      rows="6"
                                      :class="{ error: errors.ExperienceCommitment }"
                                      :aria-invalid="!!errors.ExperienceCommitment"
                                      :aria-describedby="errors.ExperienceCommitment ? 'errCommitment' : 'helpCommitment'"></textarea>

                            <div class="le-meta">
                                <span id="helpCommitment" :class="{ 'le-bad': commitmentLen < 500 }">
                                    {{ commitmentLen }} / 500 characters
                                </span>

                                <span v-if="errors.ExperienceCommitment" id="errCommitment" class="error-text" role="alert">
                                    {{ errors.ExperienceCommitment }}
                                </span>
                            </div>
                        </div>

                        <!-- Q2 -->
                        <div class="le-q">
                            <label class="le-label" for="expChallenges">
                                <span class="req">*</span>
                                Describe 2 challenges you experienced in your journey to wellness and how you overcame them:
                            </label>

                            <textarea id="expChallenges"
                                      class="le-textarea"
                                      v-model.trim="form.ExperienceChallenges"
                                      rows="6"
                                      :class="{ error: errors.ExperienceChallenges }"
                                      :aria-invalid="!!errors.ExperienceChallenges"
                                      :aria-describedby="errors.ExperienceChallenges ? 'errChallenges' : 'helpChallenges'"></textarea>

                            <div class="le-meta">
                                <span id="helpChallenges" :class="{ 'le-bad': challengesLen < 500 }">
                                    {{ challengesLen }} / 500 characters
                                </span>

                                <span v-if="errors.ExperienceChallenges" id="errChallenges" class="error-text" role="alert">
                                    {{ errors.ExperienceChallenges }}
                                </span>
                            </div>
                        </div>

                        <!-- Q3 -->
                        <div class="le-q">
                            <label class="le-label" for="expWhy">
                                <span class="req">*</span>
                                Explain why you would like to serve as a peer worker:
                            </label>

                            <textarea id="expWhy"
                                      class="le-textarea"
                                      v-model.trim="form.ExperienceWhy"
                                      rows="6"
                                      :class="{ error: errors.ExperienceWhy }"
                                      :aria-invalid="!!errors.ExperienceWhy"
                                      :aria-describedby="errors.ExperienceWhy ? 'errWhy' : 'helpWhy'"></textarea>

                            <div class="le-meta">
                                <span id="helpWhy" :class="{ 'le-bad': whyLen < 500 }">
                                    {{ whyLen }} / 500 characters
                                </span>

                                <span v-if="errors.ExperienceWhy" id="errWhy" class="error-text" role="alert">
                                    {{ errors.ExperienceWhy }}
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="le-card le-card-tight">
                        <div class="le-check">
                            <input id="selfcare" type="checkbox" v-model="form.SelfCare" />
                            <label for="selfcare" class="le-text">
                                <strong>Peer Worker Certification Self-Care</strong><br />
                                Did you review and complete the <u>Peer Worker Certification Self-Care Worksheet</u>?
                            </label>
                        </div>

                        <p class="le-text">
                            The Self-Care Worksheet helps Peer Workers reflect on their own self-care and consider if
                            Foundational might be a good option for them.
                        </p>
                    </div>

                    <div class="le-card">
                        <h4 class="le-h4">Foundational Training (Optional)</h4>

                        <p class="le-text">
                            All potential AIDS Institute (AI) Peer Workers are expected to be committed to their own wellness,
                            have dealt with their health status, and are comfortable sharing their own lived experience in order
                            to support others through their journey to wellness. For some, Foundational Training is necessary,
                            and for others, the work to be done to get to this point can be done independently.
                        </p>

                        <p class="le-reminder">
                            <strong>Reminder:</strong> Only foundational training taken after January 1st, 2012 will count toward certification.
                        </p>

                        <p class="le-info">
                            <strong>Note:</strong> Document upload has been moved to <strong>Step 5: Additional Uploads</strong>.
                        </p>
                    </div>

                    <div class="span-2 muted" v-if="saveMessage">{{ saveMessage }}</div>
                </div>

                <div v-else-if="currentStep === 3">
                    <h3 class="rc-title">Step 3: Required Courses</h3>

                    <div class="rc-card">
                        <p class="rc-text">
                            Please visit this <a href="" @click.prevent>link</a> to review required courses. All applicants in order to be
                            considered for certification must complete all core courses and specialty courses for a total of 90 or more course hours.
                            You can review courses you’ve attended by clicking this <a href="" @click.prevent>link</a>. Any courses with the status
                            attended will count for certification.
                        </p>

                        <!-- upload line intentionally removed (moved to Step 5) -->

                        <div class="rc-check">
                            <input id="requiredCourses"
                                   type="checkbox"
                                   v-model="form.RequiredCourses"
                                   :class="{ error: errors.RequiredCourses }"
                                   :aria-invalid="!!errors.RequiredCourses"
                                   :aria-describedby="errors.RequiredCourses ? 'errRequiredCourses' : null" />
                            <label for="requiredCourses">
                                <strong>Have you completed all required core courses and specialty courses?</strong>
                            </label>
                        </div>

                        <div v-if="errors.RequiredCourses" id="errRequiredCourses" class="error-text" role="alert">
                            {{ errors.RequiredCourses }}
                        </div>

                        <p class="rc-note">
                            <strong>Note:</strong> Document upload has been moved to <strong>Step 5: Additional Uploads</strong>.
                        </p>
                    </div>

                    <div class="span-2 muted" v-if="saveMessage">{{ saveMessage }}</div>
                </div>

                <div v-else-if="currentStep === 4">
                    <h3 class="sv-title">Step 4: Supervisor / Practicum</h3>

                    <!-- Upload moved to Step 5 -->
                    <p class="sv-note">
                        <strong>Note:</strong> Supervisor Practicum Evaluation upload has been moved to
                        <strong>Step 5: Additional Uploads</strong>.
                    </p>

                    <div class="sv-grid">
                        <div class="field span-2">
                            <label>Agency Name *</label>
                            <input v-model.trim="form.SupvrOrgName" type="text" :class="{ error: errors.SupvrOrgName }" />
                            <div v-if="errors.SupvrOrgName" class="error-text">{{ errors.SupvrOrgName }}</div>
                        </div>

                        <div class="field">
                            <label>Supervisor First Name *</label>
                            <input v-model.trim="form.SupvrFirstName" type="text" :class="{ error: errors.SupvrFirstName }" />
                            <div v-if="errors.SupvrFirstName" class="error-text">{{ errors.SupvrFirstName }}</div>
                        </div>

                        <div class="field">
                            <label>Supervisor Last Name *</label>
                            <input v-model.trim="form.SupvrLastName" type="text" :class="{ error: errors.SupvrLastName }" />
                            <div v-if="errors.SupvrLastName" class="error-text">{{ errors.SupvrLastName }}</div>
                        </div>

                        <div class="field span-2">
                            <label>Organization Address (Line 1) *</label>
                            <input v-model.trim="form.SupvrContAddr1" type="text" :class="{ error: errors.SupvrContAddr1 }" />
                            <div v-if="errors.SupvrContAddr1" class="error-text">{{ errors.SupvrContAddr1 }}</div>
                        </div>

                        <div class="field span-2">
                            <label>Organization Address (Line 2) (optional)</label>
                            <input v-model.trim="form.SupvrContAddr2" type="text" />
                        </div>

                        <div class="field">
                            <label>Supervisor Phone *</label>
                            <input v-model.trim="form.SupvrContPhone"
                                   type="tel"
                                   inputmode="tel"
                                   placeholder="(555) 555-5555"
                                   :class="{ error: errors.SupvrContPhone }"
                                   @blur="formatSupervisorPhone" />
                            <div v-if="errors.SupvrContPhone" class="error-text">{{ errors.SupvrContPhone }}</div>
                        </div>

                        <div class="field">
                            <label>Supervisor Email *</label>
                            <input v-model.trim="form.SupvrContEmail"
                                   type="email"
                                   inputmode="email"
                                   placeholder="name@agency.org"
                                   :class="{ error: errors.SupvrContEmail }"
                                   @blur="normalizeSupervisorEmail" />
                            <div v-if="errors.SupvrContEmail" class="error-text">{{ errors.SupvrContEmail }}</div>
                        </div>

                        <div class="sv-check span-2">
                            <label class="sv-checkline">
                                <input type="checkbox" v-model="form.ComplPracticum" />
                                <span>Have you completed a practicum?</span>
                            </label>

                            <label class="sv-checkline">
                                <input type="checkbox"
                                       v-model="form.ComplPracticumMin"
                                       :disabled="form.ComplPracticum !== true" />
                                <span>Was practicum a minimum of 500 hours?</span>
                            </label>

                            <div v-if="errors.ComplPracticumMin" class="error-text">{{ errors.ComplPracticumMin }}</div>
                        </div>

                        <div class="sv-dates span-2">
                            <div class="field">
                                <label>Dates of Practicum (Start) <span v-if="form.ComplPracticum" class="req">*</span></label>
                                <input v-model="form.PracticumBDate" type="date" :class="{ error: errors.PracticumBDate }" />
                                <div v-if="errors.PracticumBDate" class="error-text">{{ errors.PracticumBDate }}</div>
                            </div>

                            <div class="field">
                                <label>Dates of Practicum (End) <span v-if="form.ComplPracticum" class="req">*</span></label>
                                <input v-model="form.PracticumEDate" type="date" :class="{ error: errors.PracticumEDate }" />
                                <div v-if="errors.PracticumEDate" class="error-text">{{ errors.PracticumEDate }}</div>
                            </div>
                        </div>

                        <div class="span-2 muted" v-if="saveMessage">{{ saveMessage }}</div>
                    </div>
                </div>

                <!-- ✅ Step 5 -->
                <div v-else-if="currentStep === 5">
                    <h3>Step 5: Additional Uploads</h3>

                    <div v-if="uploads.message" class="muted" style="margin-bottom:10px;">
                        {{ uploads.message }}
                    </div>

                    <div v-if="errors.Step5" class="error-text" style="margin-bottom:10px;">
                        {{ errors.Step5 }}
                    </div>

                    <!-- ✅ Two clean sections -->
                    <div class="upload-sections">
                        <!-- REQUIRED -->
                        <div class="u-section">
                            <div class="u-section-title">
                                Required Items <span class="req">*</span>
                            </div>

                            <div class="u-list">
                                <div v-for="dt in requiredDocTypes" :key="dt.peerDocId" class="u-row">
                                    <div class="u-left">
                                        <div class="u-name">
                                            {{ dt.name }} <span class="req">*</span>
                                        </div>
                                        <div class="u-desc muted" v-if="dt.description">{{ dt.description }}</div>

                                        <div class="u-status">
                                            <template v-if="dt.peerDocId === 3">
                                                <span :class="ethics.signed ? 'u-ok' : 'u-bad'">
                                                    {{ ethics.signed ? "Signed" : "Not signed" }}
                                                </span>
                                                <span v-if="ethics.signedAt" class="muted">• {{ (ethics.signedAt || '').slice(0,10) }}</span>
                                            </template>

                                            <template v-else>
                                                <span :class="docsForType(dt.peerDocId).length > 0 ? 'u-ok' : 'u-bad'">
                                                    {{ docsForType(dt.peerDocId).length > 0 ? "Uploaded" : "Missing" }}
                                                </span>
                                                <span class="muted">• {{ docsForType(dt.peerDocId).length }} file(s)</span>
                                            </template>
                                        </div>
                                    </div>

                                    <div class="u-actions">
                                        <!-- Ethics -->
                                        <button v-if="dt.peerDocId === 3"
                                                class="btn btn-primary"
                                                type="button"
                                                @click="openEthicsModal"
                                                :disabled="ethics.loading">
                                            {{ ethics.signed ? "View Signed Ethics" : "Review & Sign" }}
                                        </button>

                                        <!-- Other required docs -->
                                        <div v-else class="u-upload">
                                            <input class="u-file"
                                                   type="file"
                                                   :ref="`file_${dt.peerDocId}`"
                                                   :disabled="uploads.uploading"
                                                   @change="uploadFile(dt.peerDocId, $event)" />
                                            <button class="btn btn-secondary"
                                                    type="button"
                                                    @click="triggerFile(dt.peerDocId)"
                                                    :disabled="uploads.uploading">
                                                Upload
                                            </button>
                                        </div>
                                    </div>
                                </div>

                                <!-- file management rows -->
                                <template v-for="dt in requiredDocTypesNonEthics" :key="`list_${dt.peerDocId}`">
                                    <div v-if="docsForRequiredType(dt.peerDocId).length" class="u-files">
                                        <div v-for="d in docsForType(dt.peerDocId)"
                                             :key="d.peerDocSysId || d.PeerDocSysId"
                                             class="u-file-row">

                                            <div class="u-file-left">
                                                <span class="u-chip">
                                                    {{ docTypeName(d.peerDocId ?? d.PeerDocId ?? d.docType ?? d.DocType) }}
                                                </span>

                                                <span class="u-file-name">
                                                    {{ d.fileName || d.FileName }}
                                                </span>
                                            </div>

                                            <div class="u-file-actions">
                                                <button class="btn btn-ghost"
                                                        type="button"
                                                        @click="downloadDoc(d.peerDocSysId || d.PeerDocSysId)">
                                                    View/Download
                                                </button>
                                                <button class="btn btn-ghost"
                                                        type="button"
                                                        @click="deleteDoc(d.peerDocSysId || d.PeerDocSysId)">
                                                    Remove
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </template>
                            </div>
                        </div>

                        <!-- OPTIONAL -->
                        <div class="u-section">
                            <div class="u-section-title">
                                Optional Items <span class="muted">(if applicable)</span>
                            </div>

                            <div class="u-list">
                                <div v-for="dt in optionalDocTypes" :key="dt.peerDocId" class="u-row">
                                    <div class="u-left">
                                        <div class="u-name">
                                            {{ dt.name }} <span class="muted">(Optional)</span>
                                        </div>
                                        <div class="u-desc muted" v-if="dt.description">{{ dt.description }}</div>

                                        <div class="u-status">
                                            <span :class="docsForType(dt.peerDocId).length > 0 ? 'u-ok' : 'muted'">
                                                {{ docsForType(dt.peerDocId).length > 0 ? "Uploaded" : "Not uploaded" }}
                                            </span>
                                            <span class="muted">• {{ docsForType(dt.peerDocId).length }} file(s)</span>
                                        </div>
                                    </div>

                                    <div class="u-actions">
                                        <div class="u-upload">
                                            <input class="u-file"
                                                   type="file"
                                                   :ref="`file_${dt.peerDocId}`"
                                                   :disabled="uploads.uploading"
                                                   @change="uploadFile(dt.peerDocId, $event)" />
                                            <button class="btn btn-secondary"
                                                    type="button"
                                                    @click="triggerFile(dt.peerDocId)"
                                                    :disabled="uploads.uploading">
                                                Upload
                                            </button>
                                        </div>
                                    </div>
                                </div>

                                <template v-for="dt in optionalDocTypes" :key="`list_opt_${dt.peerDocId}`">
                                    <div v-if="docsForOptionalType(dt.peerDocId).length" class="u-files">
                                        <div v-for="d in docsForOptionalType(dt.peerDocId)"
                                             :key="d.peerDocSysId || d.PeerDocSysId"
                                             class="u-file-row">

                                            <div class="u-file-left">
                                                <span class="u-chip">{{ dt.name }}</span>
                                                <span class="u-file-name">{{ d.fileName || d.FileName }}</span>
                                            </div>

                                            <div class="u-file-actions">
                                                <button class="btn btn-ghost"
                                                        type="button"
                                                        @click="downloadDoc(d.peerDocSysId || d.PeerDocSysId)">
                                                    View/Download
                                                </button>
                                                <button class="btn btn-ghost"
                                                        type="button"
                                                        @click="deleteDoc(d.peerDocSysId || d.PeerDocSysId)">
                                                    Remove
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </template>

                                <div v-if="optionalDocTypes.length === 0" class="muted">
                                    No optional documents configured.
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- ✅ Ethics Modal (keep INSIDE Step 5) -->
                    <div v-if="ethicsModalOpen" class="modal-overlay" @click.self="closeEthicsModal">
                        <div class="modal-card" role="dialog" aria-modal="true" aria-label="Code of Ethics">
                            <div class="modal-head">
                                <div class="modal-title">Code of Ethics</div>
                                <button class="btn btn-ghost" type="button" @click="closeEthicsModal">✕</button>
                            </div>

                            <div class="modal-body">
                                <div class="muted" style="margin-bottom:10px;">
                                    Review the PDF and sign below (no printing needed).
                                </div>

                                <iframe :src="ethicsPdfViewerUrl" class="ethics-frame"></iframe>

                                <div class="ethics-controls">
                                    <label class="sv-checkline">
                                        <input type="checkbox" v-model="ethics.agreed" :disabled="ethics.signed" />
                                        <span>I have read and agree to the Code of Ethics.</span>
                                    </label>

                                    <div class="field" style="margin-top:10px;">
                                        <label>Type Full Name (Signature) *</label>
                                        <input type="text"
                                               v-model.trim="ethics.signatureName"
                                               :disabled="ethics.signed"
                                               placeholder="Your full legal name" />
                                    </div>

                                    <div class="muted" v-if="ethics.signedAt" style="margin-top:6px;">
                                        Signed on: {{ (ethics.signedAt || '').slice(0, 10) }}
                                    </div>

                                    <div class="error-text" v-if="ethics.message" style="margin-top:8px;">
                                        {{ ethics.message }}
                                    </div>

                                    <div style="display:flex; gap:12px; margin-top:12px;">
                                        <button class="btn btn-secondary" type="button" @click="closeEthicsModal">
                                            Close
                                        </button>

                                        <button class="btn btn-primary"
                                                type="button"
                                                @click="signEthics"
                                                :disabled="ethics.loading || ethics.signed">
                                            {{ ethics.signed ? "Signed" : (ethics.loading ? "Loading..." : "Sign") }}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="span-2 muted" v-if="saveMessage">{{ saveMessage }}</div>
                </div>

                <!-- ✅ Step 6 -->
                <div v-else-if="currentStep === 6">
                    <h3 class="exam-title">Step 6: Peer Certification Exam</h3>

                    <div class="exam-intro-card">
                        <div>
                            <h4>Complete all required exam courses</h4>
                            <p>
                                Launch each online exam below and complete it fully. All listed courses must reach
                                <strong>100% completion</strong> before you can submit your Peer Certification application.
                            </p>
                        </div>

                        <div class="exam-summary-pill" :class="{ ready: allMandatoryExamCoursesCompleted }">
                            {{ completedMandatoryExamCount }} / {{ mandatoryExamItems.length }} Mandatory Completed
                        </div>
                    </div>

                    <div v-if="examsMessage" class="muted" style="margin-bottom: 12px;">
                        {{ examsMessage }}
                    </div>

                    <div v-if="errors.Step6" class="error-text" style="margin-bottom: 12px;">
                        {{ errors.Step6 }}
                    </div>

                    <div v-if="examsLoading" class="exam-loading-card">
                        Loading exam courses...
                    </div>

                    <template v-else>
                        <div v-if="!showScormPlayer">
                            <div class="exam-section" v-if="mandatoryExamItems.length">
                                <h4 class="exam-section-title">Mandatory Exams</h4>
                                <div class="exam-grid">
                                    <div v-for="exam in mandatoryExamItems"
                                         :key="`mandatory_${exam.subjectSysId}`"
                                         class="exam-card exam-card-mandatory"
                                         :class="{ completed: isExamCompleted(exam) }">

                                        <div class="exam-card-top">
                                            <div>
                                                <div class="exam-badge">
                                                    {{ isExamCompleted(exam) ? "Completed" : "Mandatory" }}
                                                </div>

                                                <h4>{{ exam.courseTitle }}</h4>

                                                <p class="exam-desc">
                                                    {{ exam.description || "Launch this SCORM exam and complete all required content." }}
                                                </p>
                                            </div>
                                        </div>

                                        <div class="exam-progress-row">
                                            <div class="exam-progress-meta">
                                                <span>Progress</span>
                                                <strong>{{ getExamPercent(exam) }}%</strong>
                                            </div>

                                            <div class="exam-progress-bar">
                                                <div class="exam-progress-fill" :style="{ width: `${getExamPercent(exam)}%` }"></div>
                                            </div>
                                        </div>

                                        <div class="exam-card-footer">
                                            <button class="btn btn-primary"
                                                    type="button"
                                                    @click="launchExam(exam)"
                                                    :disabled="!exam.videoUrl">
                                                {{ isExamCompleted(exam) ? "Reopen Exam" : "Launch Exam" }}
                                            </button>

                                            <div class="exam-status-text" :class="{ ok: isExamCompleted(exam) }">
                                                {{ isExamCompleted(exam) ? "Required exam completed" : "Required for submission" }}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="exam-section" v-if="optionalExamItems.length">
                                <h4 class="exam-section-title">Optional Exams</h4>
                                <div class="exam-grid">
                                    <div v-for="exam in optionalExamItems"
                                         :key="`optional_${exam.subjectSysId}`"
                                         class="exam-card exam-card-optional"
                                         :class="{ completed: isExamCompleted(exam) }">

                                        <div class="exam-card-top">
                                            <div>
                                                <div class="exam-badge">
                                                    {{ isExamCompleted(exam) ? "Completed" : "Optional" }}
                                                </div>

                                                <h4>{{ exam.courseTitle }}</h4>

                                                <p class="exam-desc">
                                                    {{ exam.description || "You may complete this additional SCORM exam if needed." }}
                                                </p>
                                            </div>
                                        </div>

                                        <div class="exam-progress-row">
                                            <div class="exam-progress-meta">
                                                <span>Progress</span>
                                                <strong>{{ getExamPercent(exam) }}%</strong>
                                            </div>

                                            <div class="exam-progress-bar">
                                                <div class="exam-progress-fill" :style="{ width: `${getExamPercent(exam)}%` }"></div>
                                            </div>
                                        </div>

                                        <div class="exam-card-footer">
                                            <button class="btn btn-secondary"
                                                    type="button"
                                                    @click="launchExam(exam)"
                                                    :disabled="!exam.videoUrl">
                                                {{ isExamCompleted(exam) ? "Reopen Exam" : "Launch Exam" }}
                                            </button>

                                            <div class="exam-status-text" :class="{ ok: isExamCompleted(exam) }">
                                                {{ isExamCompleted(exam) ? "Optional exam completed" : "Not required for submission" }}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div v-else class="exam-player-shell">
                            <div class="exam-player-header">
                                <div>
                                    <h4>{{ selectedExam?.courseTitle || "SCORM Exam" }}</h4>
                                    <p>
                                        Complete the course, then use the back button inside the player header to return here.
                                    </p>
                                </div>

                                <button class="btn btn-secondary" type="button" @click="closeScormPlayer">
                                    Close Player
                                </button>
                            </div>

                            <ScormPlayer :launchUrl="scormLaunchUrl"
                                         :registrationId="scormRegistrationId"
                                         :scoId="scormScoId"
                                         :preloadCmi="scormPreloadCmi"
                                         :title="selectedExam?.courseTitle || 'Exam Player'"
                                         @exit="handleScormExit" />
                        </div>

                        <div class="exam-bottom-note" :class="{ ready: allMandatoryExamCoursesCompleted }">
                            <strong v-if="allMandatoryExamCoursesCompleted">
                                All mandatory exam courses are complete. You can now submit your application.
                            </strong>
                            <strong v-else>
                                You must complete all mandatory exam courses for your selected certification track(s) before submitting.
                            </strong>
                        </div>
                    </template>

                    <div class="span-2 muted" v-if="saveMessage">{{ saveMessage }}</div>
                </div>

                <!-- ✅ Footer should be OUTSIDE step chain but still inside .body -->
                <div class="footer footer-dock">
                    <div class="dock">
                        <div class="dock-meta">
                            Step <strong>{{ currentStep }}</strong> of <strong>{{ steps.length }}</strong>
                        </div>

                        <div class="dock-actions">
                            <button v-if="currentStep > 1"
                                    class="btn btn-ghost"
                                    type="button"
                                    @click="prevStep"
                                    :disabled="saving">
                                ← Previous
                            </button>

                            <button class="btn btn-secondary"
                                    type="button"
                                    @click="saveDraft()"
                                    :disabled="saving">
                                {{ saving ? "Saving..." : "Save Draft" }}
                            </button>

                            <button v-if="currentStep < 6"
                                    class="btn btn-primary"
                                    type="button"
                                    @click="nextStep"
                                    :disabled="saving">
                                Next →
                            </button>

                            <button v-else
                                    class="btn btn-primary"
                                    type="button"
                                    @click="submitApplication"
                                    :disabled="saving">
                                Submit Application
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div v-if="showSubmitSuccessModal" class="modal-overlay submit-overlay">
            <div class="modal-card submit-success-modal"
                 role="dialog"
                 aria-modal="true"
                 aria-label="Application Submitted Successfully">
                <div class="submit-success-top">
                    <div class="submit-success-icon">✓</div>
                    <div>
                        <h3 class="submit-success-title">Application Submitted Successfully</h3>
                        <p class="submit-success-subtitle">Your Peer Certification application has been submitted.</p>
                    </div>
                </div>

                <div class="modal-body submit-success-body">
                    <p class="success-text">
                        Please wait for the approval. Once your application is reviewed and approved, you will be notified.
                    </p>

                    <div class="submit-success-actions">
                        <button class="btn btn-primary submit-ok-btn" type="button" @click="handleSubmitSuccessOk">
                            OK
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>import LoginModal from "@/components/LoginComponent.vue";
import ScormPlayer from "@/components/ScormPlayer.vue";

    export default {
        name: "PeerCertificationApply",
        components: { LoginModal, ScormPlayer },

        data() {
            return {
                showLogin: false,

                locked: {
                    FirstName: false,
                    LastName: false,
                    Address: false,
                    City: false,
                    State: false,
                    Zip: false,
                    WorkPhone: false,
                    WorkPhoneExt: false,
                    CellPhone: false,
                    Title: false,

                    Dob: false,
                    Gender: false,
                    CertificationTrack: false,
                },

                currentStep: 1,
                loading: false,
                saving: false,
                saveMessage: "",
                errors: {},
                showSubmitSuccessModal: false,

                steps: [
                    { id: 1, label: "Applicant Info" },
                    { id: 2, label: "Lived Experience" },
                    { id: 3, label: "Required Courses" },
                    { id: 4, label: "Supervisor / Practicum" },
                    { id: 5, label: "Additional Uploads" },
                    { id: 6, label: "Exam" },
                ],

                lookups: { ethnicities: [], races: [], educations: [], genders: [] },

                form: {
                    FirstName: "",
                    Mi: "",
                    LastName: "",
                    Email: "",
                    AltEmail: "",
                    Phone: "",
                    AltPhone: "",
                    CellPhone: "",

                    WorkPhone: "",
                    WorkPhoneExt: "",
                    PrimaryCanText: null,
                    AltCanText: null,

                    Address: "",
                    City: "",
                    State: "NY",
                    Zip: "",
                    Country: "",

                    Title: "",
                    Organization: "",
                    WorkSetting: null,
                    Occupation: null,
                    YearsCurrentOccupation: null,

                    Education: null,
                    Ethnicity: null,
                    Race: null,

                    PronounId: null,
                    WorkLocationId: null,
                    Adaneed: null,
                    Adadetails: "",

                    // PeerUser only:
                    CertificationTrack: [],
                    AgencyAffilation: "",
                    Dob: null,
                    Gender: null,
                    ExperienceCommitment: "",
                    ExperienceChallenges: "",
                    ExperienceWhy: "",
                    SelfCare: false,
                    RequiredCourses: false,

                    // Step 4 - Supervisor / Practicum
                    SupvrOrgName: "",
                    SupvrFirstName: "",
                    SupvrLastName: "",
                    SupvrContAddr1: "",
                    SupvrContAddr2: "",
                    SupvrContPhone: "",
                    SupvrContEmail: "",
                    ComplPracticum: false,
                    ComplPracticumMin: false,
                    PracticumBDate: null,
                    PracticumEDate: null,
                },

                uploads: {
                    loading: false,
                    docs: [],
                    uploading: false,
                    message: "",
                },

                ethicsModalOpen: false,
                ethics: {
                    loading: false,
                    signed: false,
                    signatureName: "",
                    agreed: false,
                    signedAt: null,
                    message: "",
                },

                examItems: [],
        examsLoading: false,
        examsMessage: "",
        selectedExam: null,
        showScormPlayer: false,
        scormRegistrationId: "",
        scormScoId: "",
        scormLaunchUrl: "",
        scormPreloadCmi: {},
        examProgress: {},

                docTypes: [],
            };
        },

        computed: {
            commitmentLen() {
                return (this.form.ExperienceCommitment || "").trim().length;
            },
            ethicsPdfUrl() {
                return "/api/PeerCertification/ethics/pdf";
            },
            ethicsPdfViewerUrl() {
                return `${this.ethicsPdfUrl}#zoom=page-width&navpanes=0&toolbar=0`;
            },
            challengesLen() {
                return (this.form.ExperienceChallenges || "").trim().length;
            },
            requiredDocTypes() {
                return (this.docTypes || []).filter((d) => d.required === true);
            },
            optionalDocTypes() {
                return (this.docTypes || []).filter((d) => d.required !== true);
            },
            whyLen() {
                return (this.form.ExperienceWhy || "").trim().length;
            },
            trackConflictMessage() {
    const tracks = (this.form.CertificationTrack || []).map(x => String(x).trim().toUpperCase());
    if (tracks.includes("HIV") && tracks.includes("PREP")) {
        return "HIV and PrEP certification tracks cannot be selected together.";
    }
    return "";
},

isPrepDisabled() {
    const tracks = (this.form.CertificationTrack || []).map(x => String(x).trim().toUpperCase());
    return tracks.includes("HIV");
},

isHivDisabled() {
    const tracks = (this.form.CertificationTrack || []).map(x => String(x).trim().toUpperCase());
    return tracks.includes("PREP");
},
            requiredDocTypesNonEthics() {
                return (this.docTypes || []).filter((d) => d.required === true && d.peerDocId !== 3);
            },
            selectedTrackCodes() {
    return (this.form.CertificationTrack || []).map(x => String(x).trim().toUpperCase());
},

mandatoryExamItems() {
    return (this.examItems || []).filter(x =>
        this.selectedTrackCodes.includes((x.trackCode || "").toUpperCase())
    );
},

optionalExamItems() {
    return (this.examItems || []).filter(x =>
        !this.selectedTrackCodes.includes((x.trackCode || "").toUpperCase())
    );
},

allMandatoryExamCoursesCompleted() {
    if (!this.mandatoryExamItems.length) return true;
    return this.mandatoryExamItems.every(x => this.isExamCompleted(x));
},

completedMandatoryExamCount() {
    return this.mandatoryExamItems.filter(x => this.isExamCompleted(x)).length;
},
            step2HasErrors() {
                return (
                    this.currentStep === 2 &&
                    (!!this.errors.ExperienceCommitment ||
                        !!this.errors.ExperienceChallenges ||
                        !!this.errors.ExperienceWhy)
                );
            },
            allExamCoursesCompleted() {
    return this.examItems.length > 0 && this.examItems.every(x => this.isExamCompleted(x));
},
completedExamCount() {
    return this.examItems.filter(x => this.isExamCompleted(x)).length;
},
        },

        async mounted() {
            if (!this.getUserGuid()) {
                this.showLogin = true;
                this.saveMessage = "Please login to apply for Peer Certification.";
                return;
            }
            await this.loadLookups();
            await this.loadApplicantInfo();
        },

        methods: {
            
            async apiFetch(url, options = {}) {
                const fullUrl = url.startsWith("/api")
                    ? url
                    : `/api${url.startsWith("/") ? "" : "/"}${url}`;

                const opts = {
                    ...options,
                    credentials: "include", // ✅ key fix (works even when ports differ)
                    headers: {
                        ...(options.headers || {}),
                        ...(options.body instanceof FormData ? {} : { Accept: "application/json" }),
                    },
                };

                const res = await fetch(fullUrl, opts);

                // If the backend returned HTML (login page / SPA fallback), throw a helpful error.
                const ct = (res.headers.get("content-type") || "").toLowerCase();
                if (ct.includes("text/html")) {
                    const txt = await res.text();
                    throw new Error(
                        `Expected JSON but got HTML. Check auth/proxy/cookies. First chars: ${txt.slice(
                            0,
                            120
                        )}`
                    );
                }

                return res;
            },

            isStep1CompleteForProgress() {
                const selectedTracks = (this.form.CertificationTrack || [])
                    .map(x => String(x).trim().toUpperCase());

                return !!(
                    this.form.FirstName?.trim() &&
                    this.form.LastName?.trim() &&
                    this.form.Address?.trim() &&
                    this.form.City?.trim() &&
                    this.form.State?.trim() &&
                    this.form.Zip?.trim() &&
                    this.form.Title?.trim() &&
                    this.form.Ethnicity !== null &&
                    this.form.Ethnicity !== undefined &&
                    this.form.Race !== null &&
                    this.form.Race !== undefined &&
                    this.form.Education !== null &&
                    this.form.Education !== undefined &&
                    this.form.Dob &&
                    this.form.Gender !== null &&
                    this.form.Gender !== undefined &&
                    selectedTracks.length > 0 &&
                    !(selectedTracks.includes("HIV") && selectedTracks.includes("PREP"))
                );
            },

            isStep2CompleteForProgress() {
                return (
                    (this.form.ExperienceCommitment || "").trim().length >= 500 &&
                    (this.form.ExperienceChallenges || "").trim().length >= 500 &&
                    (this.form.ExperienceWhy || "").trim().length >= 500
                );
            },

            isStep3CompleteForProgress() {
                return this.form.RequiredCourses === true;
            },

            isStep4CompleteForProgress() {
                const hasBasics =
                    !!this.form.SupvrOrgName?.trim() &&
                    !!this.form.SupvrFirstName?.trim() &&
                    !!this.form.SupvrLastName?.trim() &&
                    !!this.form.SupvrContAddr1?.trim() &&
                    !!this.form.SupvrContPhone?.trim() &&
                    !!this.form.SupvrContEmail?.trim();

                if (!hasBasics) return false;

                if (this.form.ComplPracticum === true) {
                    return (
                        this.form.ComplPracticumMin === true &&
                        !!this.form.PracticumBDate &&
                        !!this.form.PracticumEDate
                    );
                }

                return true;
            },

            isStep5CompleteForProgress() {
                const requiredIds = (this.docTypes || [])
                    .filter(x => x.required === true)
                    .map(x => x.peerDocId);

                return requiredIds.every(id => {
                    if (id === 3) return this.ethics.signed === true;
                    return this.docsForType(id).length > 0;
                });
            },

            isStep6CompleteForProgress() {
                return this.allMandatoryExamCoursesCompleted === true;
            },

            getApplicationPercentage() {
                const completedSteps = [
                    this.isStep1CompleteForProgress(),
                    this.isStep2CompleteForProgress(),
                    this.isStep3CompleteForProgress(),
                    this.isStep4CompleteForProgress(),
                    this.isStep5CompleteForProgress(),
                    this.isStep6CompleteForProgress()
                ].filter(Boolean).length;

                const pct = Math.round((completedSteps / 6) * 100);

                // Draft progress should never become 100.
                // 100 is reserved only for successful final submission.
                return Math.min(pct, 99);
            },

            handleSubmitSuccessOk() {
                this.showSubmitSuccessModal = false;

                this.$router.push("/peer-certification");
            },

            handleLoginSuccess(loginPayload) {
                if (loginPayload?.userId) localStorage.setItem("userId", loginPayload.userId);
                if (loginPayload?.token) localStorage.setItem("token", loginPayload.token);

                this.showLogin = false;
                this.loadLookups().then(() => this.loadApplicantInfo());
            },

            docTypeName(docTypeId) {
                const id = Number(docTypeId);
                const hit = (this.docTypes || []).find((x) => Number(x.peerDocId) === id);
                return hit?.name || `Document (${id})`;
            },

            async ensureStep5Loaded() {
                if (this.currentStep !== 5) return;
                await this.loadStep5DocTypes();
                await this.loadUploads();
                await this.loadEthicsStatus();
            },

            openEthicsModal() {
                this.ethicsModalOpen = true;
            },

            closeEthicsModal() {
                this.ethicsModalOpen = false;
            },

            triggerFile(docId) {
                const r = this.$refs[`file_${docId}`];
                const el = Array.isArray(r) ? r[0] : r;
                if (el && el.click) el.click();
            },

            docsForRequiredType(docTypeId) {
                const requiredIds = this.requiredDocTypes.map((d) => d.peerDocId);
                if (!requiredIds.includes(docTypeId)) return [];
                return this.docsForType(docTypeId);
            },

            docsForOptionalType(docTypeId) {
                const optionalIds = this.optionalDocTypes.map((d) => d.peerDocId);
                if (!optionalIds.includes(docTypeId)) return [];
                return this.docsForType(docTypeId);
            },

            async loadEthicsStatus() {
                const id = this.getUserGuid();
                if (!id) return;

                this.ethics.loading = true;
                this.ethics.message = "";

                try {
                    const res = await this.apiFetch(`/api/PeerCertification/ethics/${id}`, {
                        method: "GET",
                    });
                    if (!res.ok) return;

                    const data = await res.json();
                    this.ethics.signed = !!data?.signed;
                    this.ethics.signedAt = data?.signedAt ?? null;
                    if (data?.signatureName) this.ethics.signatureName = data.signatureName;
                } finally {
                    this.ethics.loading = false;
                }
            },
            async ensureStep6Loaded() {
    if (this.currentStep !== 6) return;
    await this.loadExamCourses();
},

async loadExamCourses() {
    const userId = this.getUserGuid();
    if (!userId) return;

    this.examsLoading = true;
    this.examsMessage = "";

    try {
        const subjectIds = [1010, 1005, 1003, 1007, 5];

        const res = await this.apiFetch(
            `/api/PeerCertification/exam-courses/${userId}?subjectIds=${subjectIds.join(",")}`,
            { method: "GET" }
        );

        if (!res.ok) {
            this.examsMessage = `Failed to load exam courses: ${await res.text()}`;
            this.examItems = [];
            return;
        }

        const data = await res.json();
        const rows = Array.isArray(data) ? data : (data?.$values || []);

        this.examItems = rows.map(x => ({
    subjectSysId: x.subjectSysId,
    courseSysId: x.courseSysId,
    courseTitle: x.courseTitle,
    description: x.description,
    videoUrl: x.videoUrl,
    scormId: x.scormId ?? x.courseSysId ?? x.subjectSysId,
    scoId: x.scoId ?? "",
    trackCode: (x.trackCode || "").toUpperCase(),
    completed: x.completed === true,
    percent: Number(x.percent || 0)
}));

        const map = {};
        this.examItems.forEach(x => {
            map[x.subjectSysId] = {
                completed: x.completed === true,
                percent: Number(x.percent || 0)
            };
        });
        this.examProgress = map;
    } catch (e) {
        this.examsMessage = e?.message || "Failed to load exam courses.";
        this.examItems = [];
    } finally {
        this.examsLoading = false;
    }
},

isExamCompleted(exam) {
    const row = this.examProgress?.[exam.subjectSysId];
    if (!row) return exam.completed === true || Number(exam.percent || 0) >= 100;
    return row.completed === true || Number(row.percent || 0) >= 100;
},

getExamPercent(exam) {
    const row = this.examProgress?.[exam.subjectSysId];
    if (!row) return Math.min(100, Math.max(0, Number(exam.percent || 0)));
    return Math.min(100, Math.max(0, Number(row.percent || 0)));
},

async launchExam(exam) {
    if (!exam?.videoUrl) {
        this.examsMessage = "This exam does not have a valid SCORM launch URL.";
        return;
    }

    if (!exam?.courseSysId) {
        this.examsMessage = "This exam is not mapped to a course record.";
        return;
    }

    try {
        this.examsMessage = "";
        this.selectedExam = exam;

        const userId = this.getUserGuid();

        const regRes = await this.apiFetch("/api/PeerCertification/register-exam-course", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                userId,
                courseSysId: exam.courseSysId
            })
        });

        if (!regRes.ok) {
            this.examsMessage = `Failed to register exam course: ${await regRes.text()}`;
            return;
        }

        const initRes = await this.apiFetch("/api/scorm/runtime/init", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                userId,
                scormId: exam.courseSysId,
                scoId: exam.scoId || null,
                forceNewAttempt: false
            })
        });

        if (!initRes.ok) {
            this.examsMessage = `Failed to initialize exam player: ${await initRes.text()}`;
            return;
        }

        const initData = await initRes.json();

        this.scormRegistrationId = initData.registrationId || "";
        this.scormScoId = initData.scoId || "";
        this.scormPreloadCmi = initData.preloadCmi || {};
        this.scormLaunchUrl = exam.videoUrl;
        this.showScormPlayer = true;
    } catch (e) {
        this.examsMessage = e?.message || "Unable to launch exam.";
    }
},

async handleScormExit() {
    this.showScormPlayer = false;
    this.scormLaunchUrl = "";
    this.scormRegistrationId = "";
    this.scormScoId = "";
    this.scormPreloadCmi = {};
    await this.refreshExamProgress();
},

closeScormPlayer() {
    this.showScormPlayer = false;
    this.scormLaunchUrl = "";
    this.scormRegistrationId = "";
    this.scormScoId = "";
    this.scormPreloadCmi = {};
},

async refreshExamProgress() {
    const userId = this.getUserGuid();
    if (!userId || !this.examItems.length) return;

    try {
        const subjectIds = this.examItems.map(x => x.subjectSysId);
        const res = await this.apiFetch(
            `/api/PeerCertification/exam-courses/${userId}?subjectIds=${subjectIds.join(",")}`,
            { method: "GET" }
        );

        if (!res.ok) return;

        const data = await res.json();
        const rows = Array.isArray(data) ? data : (data?.$values || []);

        const map = {};
        rows.forEach(x => {
            map[x.subjectSysId] = {
                completed: x.completed === true,
                percent: Number(x.percent || 0)
            };
        });

        this.examProgress = map;
    } catch {
        // ignore refresh failure
    }
},

validateStep6() {
    const e = { ...this.errors };

    if (!this.mandatoryExamItems.length) {
        e.Step6 = "No mandatory exam courses are configured for the selected certification track.";
    } else if (!this.allMandatoryExamCoursesCompleted) {
        e.Step6 = "Please complete all mandatory exam courses before submitting.";
    } else {
        delete e.Step6;
    }

    this.errors = e;
    return !e.Step6;
},

            async signEthics() {
                const id = this.getUserGuid();
                if (!id) return;

                this.ethics.message = "";

                if (this.ethics.agreed !== true) {
                    this.ethics.message = "Please confirm you agree to the Code of Ethics.";
                    return;
                }
                if (!this.ethics.signatureName || this.ethics.signatureName.trim().length < 3) {
                    this.ethics.message = "Please type your full name to sign.";
                    return;
                }

                try {
                    const res = await this.apiFetch(`/api/PeerCertification/ethics/${id}`, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({
                            signatureName: this.ethics.signatureName.trim(),
                            agreed: true,
                        }),
                    });

                    if (!res.ok) {
                        this.ethics.message = `Sign failed: ${await res.text()}`;
                        return;
                    }

                    const data = await res.json();
                    this.ethics.signed = true;
                    this.ethics.signedAt = data?.signedAt ?? new Date().toISOString();
                    this.ethics.message = "Signed successfully.";
                } catch (e) {
                    this.ethics.message = e?.message || "Sign failed (network error).";
                }
            },

            async loadStep5DocTypes() {
                const requiredOrder = [3, 2, 4, 8, 6, 7];

                const fallback = [
                    { peerDocId: 3, name: "Code of Ethics", required: true, description: null },
                    { peerDocId: 2, name: "Resume", required: true, description: null },
                    { peerDocId: 4, name: "Foundational Training Certificate", required: false, description: null },
                    {
                        peerDocId: 8,
                        name: "Safe Talk Suicide Alertness Training Certificate",
                        required: false,
                        description: null,
                    },
                    { peerDocId: 6, name: "Other Certificates / Diplomas", required: false, description: null },
                    { peerDocId: 7, name: "Supervisor Practicum Evaluation Form", required: true, description: null },
                ];

                try {
                    const res = await this.apiFetch("/api/PeerCertification/step5-doc-types");
                    let api = [];
                    if (res.ok) api = await res.json();

                    const safeMerge = (base, incoming) => {
                        const out = { ...(base || {}) };
                        for (const [k, v] of Object.entries(incoming || {})) {
                            if (v === null || v === undefined) continue;
                            if (typeof v === "string" && v.trim() === "") continue;
                            out[k] = v;
                        }
                        return out;
                    };

                    const map = new Map();
                    for (const f of fallback) map.set(f.peerDocId, f);

                    for (const a of api || []) {
                        const id = Number(a.peerDocId ?? a.PeerDocId);
                        if (!id) continue;

                        const normalized = {
                            peerDocId: id,
                            name: a.name ?? a.Name,
                            required: a.required ?? a.Required,
                            description: a.description ?? a.Description,
                        };

                        map.set(id, safeMerge(map.get(id), normalized));
                    }

                    const allIds = Array.from(map.keys());
                    const orderedIds = [
                        ...requiredOrder.filter((id) => map.has(id)),
                        ...allIds.filter((id) => !requiredOrder.includes(id)),
                    ];

                    // ✅ FIX: do NOT overwrite docTypes again (keeps optional docs too)
                    this.docTypes = orderedIds.map((id) => map.get(id)).filter(Boolean);
                } catch {
                    this.docTypes = fallback;
                }
            },

            // keep only digits (and optional leading +1 handling)
            digitsOnly(v) {
                return (v || "").replace(/\D/g, "");
            },

            isValidUsPhone(v) {
                const d = this.digitsOnly(v);
                if (d.length === 10) return true;
                if (d.length === 11 && d.startsWith("1")) return true;
                return false;
            },

            formatUsPhone(v) {
                let d = this.digitsOnly(v);
                if (d.length === 11 && d.startsWith("1")) d = d.slice(1);
                if (d.length !== 10) return v;

                const a = d.slice(0, 3);
                const b = d.slice(3, 6);
                const c = d.slice(6);
                return `(${a}) ${b}-${c}`;
            },

            formatSupervisorPhone() {
                if (this.form.SupvrContPhone && this.isValidUsPhone(this.form.SupvrContPhone)) {
                    this.form.SupvrContPhone = this.formatUsPhone(this.form.SupvrContPhone);
                }
            },

            isValidEmail(v) {
                const email = (v || "").trim();
                return /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/i.test(email);
            },

            normalizeSupervisorEmail() {
                if (this.form.SupvrContEmail) {
                    this.form.SupvrContEmail = this.form.SupvrContEmail.trim().toLowerCase();
                }
            },

            getUserGuid() {
                return localStorage.getItem("userId");
            },

            unwrapDotNetList(data) {
                if (Array.isArray(data)) return data;
                if (data && Array.isArray(data.$values)) return data.$values;
                return [];
            },

            validateStep2() {
                const e = { ...this.errors };

                const min500Required = (key, message) => {
                    const val = (this.form[key] || "").trim();
                    if (!val) {
                        e[key] = message;
                        return;
                    }
                    if (val.length < 500) {
                        e[key] = `${message} (Minimum 500 characters required. Current: ${val.length})`;
                        return;
                    }
                    delete e[key];
                };

                min500Required(
                    "ExperienceCommitment",
                    "Please describe your personal commitment to wellness related to your lived experience (HIV, HCV, or harm reduction)."
                );
                min500Required(
                    "ExperienceChallenges",
                    "Please describe two challenges you experienced in your journey to wellness and how you overcame them."
                );
                min500Required("ExperienceWhy", "Please explain why you would like to serve as a peer worker.");

                this.errors = e;
                return !e.ExperienceCommitment && !e.ExperienceChallenges && !e.ExperienceWhy;
            },

            validateStep3() {
                const e = { ...this.errors };

                if (this.form.RequiredCourses !== true) {
                    e.RequiredCourses =
                        "Please confirm you have completed all required core and specialty courses.";
                } else {
                    delete e.RequiredCourses;
                }

                this.errors = e;
                return !e.RequiredCourses;
            },

            validateStep4() {
                const e = { ...this.errors };

                const req = (k, label) => {
                    const v = this.form[k];
                    if (v === null || v === undefined || String(v).trim() === "") {
                        e[k] = `${label} is required.`;
                        return false;
                    }
                    delete e[k];
                    return true;
                };

                req("SupvrOrgName", "Agency Name");
                req("SupvrFirstName", "Supervisor First Name");
                req("SupvrLastName", "Supervisor Last Name");
                req("SupvrContAddr1", "Organization Address (Line 1)");

                if (req("SupvrContPhone", "Supervisor Phone")) {
                    const d = this.digitsOnly(this.form.SupvrContPhone);
                    if (!(d.length === 10 || (d.length === 11 && d.startsWith("1")))) {
                        e.SupvrContPhone = "Please enter a valid phone number (10 digits).";
                    } else {
                        this.form.SupvrContPhone = d.length === 11 ? d.slice(1) : d;
                        delete e.SupvrContPhone;
                    }
                }

                if (req("SupvrContEmail", "Supervisor Email")) {
                    if (!this.isValidEmail(this.form.SupvrContEmail)) {
                        e.SupvrContEmail = "Please enter a valid email address. Example: name@agency.org.";
                    } else {
                        delete e.SupvrContEmail;
                    }
                }

                if (this.form.ComplPracticum === true) {
                    if (this.form.ComplPracticumMin !== true) {
                        e.ComplPracticumMin = "Please confirm whether your practicum was at least 500 hours.";
                    } else {
                        delete e.ComplPracticumMin;
                    }

                    if (!this.form.PracticumBDate) e.PracticumBDate = "Practicum start date is required.";
                    else delete e.PracticumBDate;

                    if (!this.form.PracticumEDate) e.PracticumEDate = "Practicum end date is required.";
                    else delete e.PracticumEDate;

                    if (this.form.PracticumBDate && this.form.PracticumEDate) {
                        const s = new Date(this.form.PracticumBDate);
                        const en = new Date(this.form.PracticumEDate);
                        if (en < s) {
                            e.PracticumEDate = "Practicum end date cannot be before the start date.";
                        }
                    }
                } else {
                    delete e.ComplPracticumMin;
                    delete e.PracticumBDate;
                    delete e.PracticumEDate;
                }

                this.errors = e;

                const step4Keys = [
                    "SupvrOrgName",
                    "SupvrFirstName",
                    "SupvrLastName",
                    "SupvrContAddr1",
                    "SupvrContPhone",
                    "SupvrContEmail",
                    "ComplPracticumMin",
                    "PracticumBDate",
                    "PracticumEDate",
                ];

                return step4Keys.every((k) => !e[k]);
            },

            validateStep5() {
                const e = { ...this.errors };
                const requiredIds = (this.docTypes || []).filter((x) => x.required).map((x) => x.peerDocId);

                const missing = requiredIds.filter((id) => {
                    if (id === 3) return this.ethics.signed !== true;
                    return this.docsForType(id).length === 0;
                });

                if (missing.length > 0) e.Step5 = "Please complete all required items before proceeding.";
                else delete e.Step5;

                this.errors = e;
                return !e.Step5;
            },

            async loadUploads() {
                const id = this.getUserGuid();
                if (!id) return;

                this.uploads.loading = true;
                try {
                    const res = await this.apiFetch(`/api/PeerCertification/uploads/${id}`);
                    if (!res.ok) return;

                    const data = await res.json();
                    const rawDocs = data?.docs ?? data?.Docs ?? data;
                    this.uploads.docs = this.unwrapDotNetList(rawDocs);
                } finally {
                    this.uploads.loading = false;
                }
            },

            docsForType(docType) {
                const docs = this.unwrapDotNetList(this.uploads.docs);
                const t = Number(docType);

                return docs.filter((d) => {
                    const id = Number(d.peerDocId ?? d.PeerDocId ?? d.docType ?? d.DocType);
                    return id === t;
                });
            },

            async uploadFile(docType, evt) {
                const id = this.getUserGuid();
                const file = evt?.target?.files?.[0];
                if (!id || !file) return;

                const fd = new FormData();
                fd.append("file", file);
                fd.append("docType", docType);

                this.uploads.uploading = true;
                this.uploads.message = "";

                try {
                    const res = await this.apiFetch(`/api/PeerCertification/uploads/${id}`, {
                        method: "POST",
                        body: fd,
                    });

                    if (!res.ok) {
                        this.uploads.message = `Upload failed: ${await res.text()}`;
                        return;
                    }

                    const data = await res.json();
                    const rawDocs = data?.docs ?? data?.Docs ?? data;
                    this.uploads.docs = this.unwrapDotNetList(rawDocs);
                    this.uploads.message = "Uploaded successfully.";
                } catch (e) {
                    this.uploads.message = e?.message || "Upload failed (network error).";
                } finally {
                    this.uploads.uploading = false;
                    if (evt?.target) evt.target.value = "";
                }
            },

            downloadDoc(peerDocSysId) {
                window.open(`/api/PeerCertification/uploads/download/${peerDocSysId}`, "_blank");
            },

            async deleteDoc(peerDocSysId) {
                const id = this.getUserGuid();
                if (!id) return;

                if (!confirm("Remove this document?")) return;

                const res = await this.apiFetch(`/api/PeerCertification/uploads/${id}/${peerDocSysId}`, {
                    method: "DELETE",
                });

                if (!res.ok) {
                    this.uploads.message = `Delete failed: ${await res.text()}`;
                    return;
                }

                const data = await res.json();
                const rawDocs = data?.docs ?? data?.Docs ?? data;
                this.uploads.docs = this.unwrapDotNetList(rawDocs);
                this.uploads.message = "Removed.";
            },

            normalizeLookupList(data) {
                const items = this.unwrapDotNetList(data);
                return items
                    .map((x) => ({
                        code: x.code ?? x.Code ?? x.id ?? x.Id ?? null,
                        value: x.value ?? x.Value ?? x.label ?? x.Label ?? "",
                    }))
                    .filter((x) => x.code !== null);
            },

            async loadLookups() {
                try {
                    const [eth, race, edu] = await Promise.all([
                        this.apiFetch("/api/Lookup/ethnicities"),
                        this.apiFetch("/api/Lookup/races"),
                        this.apiFetch("/api/Lookup/educations"),
                    ]);

                    this.lookups.ethnicities = this.normalizeLookupList(eth.ok ? await eth.json() : null);
                    this.lookups.races = this.normalizeLookupList(race.ok ? await race.json() : null);
                    this.lookups.educations = this.normalizeLookupList(edu.ok ? await edu.json() : null);

                    const genRes = await this.apiFetch("/api/PeerCertification/lookups");
                    const genJson = genRes.ok ? await genRes.json() : null;
                    this.lookups.genders = this.normalizeLookupList(genJson?.genders);
                } catch {
                    // ignore
                }
            },

            validateStep1() {
                const e = {};

                const reqText = (k, label) => {
                    const v = this.form[k];
                    if (v === null || v === undefined || String(v).trim() === "") e[k] = `${label} is required.`;
                };
                const reqSelect = (k, label) => {
                    const v = this.form[k];
                    if (v === null || v === undefined || v === "") e[k] = `${label} is required.`;
                };

                reqText("FirstName", "First Name");
                reqText("LastName", "Last Name");
                reqText("Address", "Address");
                reqText("City", "City");
                reqText("State", "State");
                reqText("Zip", "Zip");
                reqText("Title", "Title");

                reqSelect("Ethnicity", "Ethnicity");
                reqSelect("Race", "Race");
                reqSelect("Education", "Education");
const selectedTracks = (this.form.CertificationTrack || []).map(x => String(x).trim().toUpperCase());

if (!Array.isArray(this.form.CertificationTrack) || this.form.CertificationTrack.length === 0) {
    e.CertificationTrack = "Please select at least one Certification Track.";
} else if (selectedTracks.includes("HIV") && selectedTracks.includes("PREP")) {
    e.CertificationTrack = "HIV and PrEP certification tracks cannot be selected together.";
}               reqSelect("Dob", "Date of Birth");
                reqSelect("Gender", "Gender");

                this.errors = e;
                return Object.keys(e).length === 0;
            },

            async loadApplicantInfo() {
    const id = this.getUserGuid();
    if (!id) {
        this.showLogin = true;
        this.saveMessage = "Please login to continue.";
        return;
    }

    this.loading = true;
    this.saveMessage = "";
    this.errors = {};

    try {
        const res = await this.apiFetch(`/api/PeerCertification/applicant-info/${id}`, {
            method: "GET",
        });

        if (res.status === 401) {
            this.showLogin = true;
            this.saveMessage = "Session expired. Please login again.";
            return;
        }
        if (!res.ok) {
            this.saveMessage = `Load failed: ${await res.text()}`;
            return;
        }

        const data = await res.json();

        const toCamel = (s) => (s ? s.charAt(0).toLowerCase() + s.slice(1) : s);
        const toPascal = (s) => (s ? s.charAt(0).toUpperCase() + s.slice(1) : s);

        const pick = (obj, ...keys) => {
            for (const k of keys) {
                if (obj && obj[k] !== undefined && obj[k] !== null) return obj[k];
            }
            return undefined;
        };

        const map = {
            FirstName: "firstName",
            Mi: "mi",
            LastName: "lastName",
            Email: "email",
            AltEmail: "altEmail",
            Phone: "phone",
            AltPhone: "altPhone",
            CellPhone: "cellPhone",
            WorkPhone: "workPhone",
            WorkPhoneExt: "workPhoneExt",
            PrimaryCanText: "primaryCanText",
            AltCanText: "altCanText",
            Address: "address",
            City: "city",
            State: "state",
            Zip: "zip",
            Country: "country",
            Title: "title",
            Organization: "organization",
            WorkSetting: "workSetting",
            Education: "education",
            Ethnicity: "ethnicity",
            Race: "race",
            Occupation: "occupation",
            YearsCurrentOccupation: "yearsCurrentOccupation",
            PronounId: "pronounId",
            WorkLocationId: "workLocationId",
            Adaneed: "adaneed",
            Adadetails: "adadetails",
            Dob: "dob",
            Gender: "gender",
            AgencyAffilation: "agencyAffilation",

            ExperienceCommitment: "experienceCommitment",
            ExperienceChallenges: "experienceChallenges",
            ExperienceWhy: "experienceWhy",
            SelfCare: "selfCare",
            RequiredCourses: "requiredCourses",
            ApplicationPercentage: "applicationPercentage",

            SupvrOrgName: "supvrOrgName",
            SupvrFirstName: "supvrFirstName",
            SupvrLastName: "supvrLastName",
            SupvrContAddr1: "supvrContAddr1",
            SupvrContAddr2: "supvrContAddr2",
            SupvrContPhone: "supvrContPhone",
            SupvrContEmail: "supvrContEmail",
            ComplPracticum: "complPracticum",
            ComplPracticumMin: "complPracticumMin",
            PracticumBDate: "practicumBDate",
            PracticumEDate: "practicumEDate",
        };

        Object.keys(this.form).forEach((k) => {
            if (k === "CertificationTrack") return;

            const apiKeyFromMap = map[k];
            const camelFromForm = toCamel(k);
            const pascalFromMap = apiKeyFromMap ? toPascal(apiKeyFromMap) : undefined;

            const val = pick(data, apiKeyFromMap, camelFromForm, pascalFromMap, k);
            if (val !== undefined) this.form[k] = val;
        });

        const rawTracks =
    data.certificationTrack ??
    data.CertificationTrack ??
    [];

let normalizedTracks = [];

if (Array.isArray(rawTracks)) {
    normalizedTracks = rawTracks;
} else if (rawTracks && Array.isArray(rawTracks.$values)) {
    normalizedTracks = rawTracks.$values;
} else if (typeof rawTracks === "string" && rawTracks.trim()) {
    normalizedTracks = [rawTracks];
}

this.form.CertificationTrack = normalizedTracks
    .map(x => String(x).trim().toUpperCase())
    .filter(Boolean);
        const toDateOnly = (v) => (v ? String(v).slice(0, 10) : null);
        this.form.PracticumBDate = toDateOnly(this.form.PracticumBDate);
        this.form.PracticumEDate = toDateOnly(this.form.PracticumEDate);

        const toBool = (v) => v === true || v === "true" || v === 1;
        this.form.ComplPracticum = toBool(this.form.ComplPracticum);
        this.form.ComplPracticumMin = toBool(this.form.ComplPracticumMin);

        if (data?.dob) {
            const d = new Date(data.dob);
            const yyyy = d.getFullYear();
            const mm = String(d.getMonth() + 1).padStart(2, "0");
            const dd = String(d.getDate()).padStart(2, "0");
            this.form.Dob = `${yyyy}-${mm}-${dd}`;
        }

        if (!this.form.State || String(this.form.State).trim() === "") {
            this.form.State = "NY";
        }

        const lockIfHasValue = (key) => {
            const v = this.form[key];
            if (v === null || v === undefined) return;
            if (typeof v === "string" && v.trim() === "") return;
            if (Object.prototype.hasOwnProperty.call(this.locked, key)) this.locked[key] = true;
        };

        ["FirstName", "LastName"].forEach(lockIfHasValue);
    } catch (e) {
        this.saveMessage = e?.message || "Load failed (network error).";
    } finally {
        this.loading = false;
    }
},

            buildPayloadForCurrentStep() {
                const base = {
                    CertificationTrack: this.form.CertificationTrack,
                    ApplicationPercentage: this.getApplicationPercentage()
                };
                if (this.currentStep === 1) {
                    return {
                        ...base,
                        FirstName: this.form.FirstName,
                        Mi: this.form.Mi,
                        LastName: this.form.LastName,
                        Email: this.form.Email,
                        AltEmail: this.form.AltEmail,
                        Phone: this.form.Phone,
                        AltPhone: this.form.AltPhone,
                        CellPhone: this.form.CellPhone,

                        WorkPhone: this.form.WorkPhone,
                        WorkPhoneExt: this.form.WorkPhoneExt,
                        PrimaryCanText: this.form.PrimaryCanText,
                        AltCanText: this.form.AltCanText,

                        Address: this.form.Address,
                        City: this.form.City,
                        State: this.form.State,
                        Zip: this.form.Zip,
                        Country: this.form.Country,

                        Title: this.form.Title,
                        Organization: this.form.Organization,
                        WorkSetting: this.form.WorkSetting,
                        Occupation: this.form.Occupation,
                        YearsCurrentOccupation: this.form.YearsCurrentOccupation,

                        Education: this.form.Education,
                        Ethnicity: this.form.Ethnicity,
                        Race: this.form.Race,

                        PronounId: this.form.PronounId,
                        WorkLocationId: this.form.WorkLocationId,

                        Adaneed: this.form.Adaneed,
                        Adadetails: this.form.Adadetails,

                        Dob: this.form.Dob,
                        Gender: this.form.Gender,
                        AgencyAffilation: this.form.AgencyAffilation,
                    };
                }

                if (this.currentStep === 2) {
                    return {
                        ...base,
                        ExperienceCommitment: this.form.ExperienceCommitment,
                        ExperienceChallenges: this.form.ExperienceChallenges,
                        ExperienceWhy: this.form.ExperienceWhy,
                        SelfCare: this.form.SelfCare,
                    };
                }

                if (this.currentStep === 3) {
                    return { ...base, RequiredCourses: this.form.RequiredCourses };
                }

                if (this.currentStep === 4) {
                    return {
                        ...base,
                        SupvrOrgName: this.form.SupvrOrgName,
                        SupvrFirstName: this.form.SupvrFirstName,
                        SupvrLastName: this.form.SupvrLastName,
                        SupvrContAddr1: this.form.SupvrContAddr1,
                        SupvrContAddr2: this.form.SupvrContAddr2,
                        SupvrContPhone: this.form.SupvrContPhone,
                        SupvrContEmail: this.form.SupvrContEmail,
                        ComplPracticum: this.form.ComplPracticum,
                        ComplPracticumMin: this.form.ComplPracticumMin,
                        PracticumBDate: this.form.PracticumBDate,
                        PracticumEDate: this.form.PracticumEDate,
                    };
                }

                return base;
            },

            async saveDraft(silent = false) {
                const id = this.getUserGuid();
                if (!id) {
                    this.showLogin = true;
                    this.saveMessage = "Please login to save.";
                    return false;
                }

                if (this.currentStep === 1) {
                    if (!this.validateStep1()) {
                        this.saveMessage = "Please complete the required fields.";
                        return false;
                    }
                }

                if (this.currentStep === 2) {
                    if (!this.validateStep2()) {
                        this.saveMessage = "Please complete all three responses (min 500 characters each).";
                        return false;
                    }
                }

                if (this.currentStep === 3) {
                    if (!this.validateStep3()) {
                        this.saveMessage = "Please complete the required confirmation.";
                        return false;
                    }
                }

                if (this.currentStep === 4) {
                    if (!this.validateStep4()) {
                        this.saveMessage = "Please complete the required fields in Step 4.";
                        return false;
                    }
                }

                if (this.currentStep === 5) {
                    if (!this.validateStep5()) {
                        this.saveMessage = this.errors.Step5;
                        return false;
                    }
                }

                this.saving = true;
                if (!silent) this.saveMessage = "";

                try {
                    const payload = this.buildPayloadForCurrentStep();

                    const res = await this.apiFetch(`/api/PeerCertification/applicant-info/${id}`, {
                        method: "PUT",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(payload),
                    });

                    if (res.status === 401) {
                        this.showLogin = true;
                        this.saveMessage = "Session expired. Please login again.";
                        return false;
                    }

                    if (!res.ok) {
                        this.saveMessage = `Save failed: ${await res.text()}`;
                        return false;
                    }

                    if (!silent) this.saveMessage = "Saved successfully.";
                    return true;
                } catch (e) {
                    this.saveMessage = e?.message || "Save failed (network error).";
                    return false;
                } finally {
                    this.saving = false;
                }
            },

            async goToStep(step) {
    if (step === this.currentStep) return;

    const saved = await this.saveDraft(true);
    if (!saved) return;

    this.currentStep = step;
    await this.ensureStep5Loaded();
    await this.ensureStep6Loaded();
    window.scrollTo({ top: 0, behavior: "smooth" });
},

            async prevStep() {
    const saved = await this.saveDraft(true);
    if (!saved) return;

    if (this.currentStep > 1) this.currentStep--;
    await this.ensureStep5Loaded();
    await this.ensureStep6Loaded();
    window.scrollTo({ top: 0, behavior: "smooth" });
},

            async nextStep() {
    const saved = await this.saveDraft(true);
    if (!saved) return;

    if (this.currentStep < 6) this.currentStep++;
    await this.ensureStep5Loaded();
    await this.ensureStep6Loaded();
    window.scrollTo({ top: 0, behavior: "smooth" });
},

            async submitApplication() {
                const id = this.getUserGuid();
                if (!id) {
                    this.showLogin = true;
                    this.saveMessage = "Please login to submit.";
                    return;
                }

                const saved = await this.saveDraft(true);
                if (!saved) return;

                if (!this.validateStep6()) {
                    this.saveMessage = this.errors.Step6;
                    return;
                }

                this.saving = true;
                this.saveMessage = "";

                try {
                    const res = await this.apiFetch(`/api/PeerCertification/submit/${id}`, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" }
                    });

                    if (!res.ok) {
                        this.saveMessage = `Submit failed: ${await res.text()}`;
                        return;
                    }

                    await res.json();

                    this.showSubmitSuccessModal = true;
                } catch (e) {
                    this.saveMessage = e?.message || "Submit failed (network error).";
                } finally {
                    this.saving = false;
                }
            },
        },

        watch: {
    currentStep: {
        immediate: true,
        handler() {
            this.ensureStep5Loaded();
            this.ensureStep6Loaded();
        },
    },

    "form.CertificationTrack": {
        deep: true,
        handler(newVal) {
            const tracks = (newVal || []).map(x => String(x).trim().toUpperCase());

            if (tracks.includes("HIV") && tracks.includes("PREP")) {
                this.errors = {
                    ...this.errors,
                    CertificationTrack: "HIV and PrEP certification tracks cannot be selected together."
                };
            } else if (this.errors.CertificationTrack === "HIV and PrEP certification tracks cannot be selected together.") {
                const updated = { ...this.errors };
                delete updated.CertificationTrack;
                this.errors = updated;
            }
        },
    },
},
    };</script>

<style scoped>
    .apply-wrap {
        padding: 18px;
        display: flex;
        justify-content: center;
    }

    /* ✅ Wider, less wasted space */
    .apply-card {
        width: min(1440px, calc(100% - 24px));
        background: #fff;
        border-radius: 18px;
        border: 1px solid #e5e7eb;
        box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08);
        overflow: hidden;
    }

    /* Header slightly tighter */
    .header {
        background: #43285d;
        padding: 18px 22px;
        color: #fff;
    }

        .header h1 {
            margin: 0;
            font-size: 22px;
            font-weight: 800;
        }

        .header p {
            margin: 6px 0 0;
            opacity: 0.9;
            font-size: 13px;
        }

    /* ✅ Stepper: fit all 6 in one row */
    .stepper {
        display: grid;
        grid-template-columns: repeat(6, minmax(0, 1fr));
        gap: 10px;
        padding: 14px 18px;
        border-bottom: 1px solid #e5e7eb;
        background: #fafafa;
    }

    .step {
        display: flex;
        align-items: center;
        gap: 10px;
        cursor: pointer;
        padding: 8px 10px;
        border-radius: 12px;
        transition: background 0.15s ease, transform 0.15s ease;
        min-width: 0;
    }

        .step:hover {
            background: rgba(67, 40, 93, 0.08);
            transform: translateY(-1px);
        }

    /* hide the old line div since we’re using grid */
    .line {
        display: none;
    }

    .circle {
        width: 30px;
        height: 30px;
        border-radius: 999px;
        display: flex;
        align-items: center;
        justify-content: center;
        border: 2px solid #d1d5db;
        font-weight: 800;
        font-size: 12px;
        color: #374151;
        background: #fff;
        flex: 0 0 auto;
    }

    .label {
        font-size: 12.5px;
        font-weight: 700;
        color: #374151;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    /* active */
    .step.active .circle {
        border-color: #43285d;
        color: #43285d;
    }

    .step.active .label {
        color: #43285d;
    }

    /* done */
    .step.done .circle {
        background: #43285d;
        border-color: #43285d;
        color: #fff;
    }

    /* Body */
    .body {
        padding: 18px 22px;
        min-height: 260px;
    }

    /* ✅ Better grid density */
    .form-grid {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 12px 14px;
        margin-top: 12px;
    }

    .field label {
        display: block;
        font-size: 12px;
        font-weight: 800;
        color: #374151;
        margin-bottom: 6px;
    }

    .field input,
    .field select {
        width: 100%;
        border: 1px solid #d1d5db;
        border-radius: 12px;
        padding: 11px 12px;
        outline: none;
        background: #fff;
        transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }

        .field input:focus,
        .field select:focus {
            border-color: rgba(67, 40, 93, 0.6);
            box-shadow: 0 0 0 4px rgba(67, 40, 93, 0.12);
        }

    .span-2 {
        grid-column: span 2;
    }

    .muted {
        color: #6b7280;
        font-size: 13px;
    }

    .phone-row {
        display: grid;
        grid-template-columns: 1.4fr 0.6fr;
        gap: 12px 14px;
        align-items: end;
    }

        .phone-row .cell-line {
            grid-column: 1 / -1; /* full width on second line */
        }

    /* ✅ Footer centered + sticky-like feel */
    .footer {
        padding: 14px 22px;
        border-top: 1px solid #e5e7eb;
        background: #fff;
        display: flex;
        justify-content: center; /* ✅ center everything */
    }

    .footer-center {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 10px;
    }

    /* ✅ Small helper text */
    .step-hint {
        font-size: 12px;
        color: #6b7280;
    }

    .action-group {
        display: inline-flex;
        align-items: center;
        gap: 14px; 
        padding: 14px 16px;
        border-radius: 999px;
        background: #f9fafb;
        border: 1px solid #e5e7eb;
        box-shadow: 0 10px 20px rgba(15, 23, 42, 0.06);
    }

        .action-group .btn {
            height: 44px;
            padding: 10px 22px;
        }

            .action-group .btn + .btn {
                position: relative;
            }

                .action-group .btn + .btn::before {
                    content: "";
                    position: absolute;
                    left: -8px; /* divider sits in the gap */
                    top: 10px;
                    height: 24px;
                    width: 1px;
                    background: #e5e7eb;
                }

    /* ✅ Responsive: stack buttons if needed */
    @media (max-width: 620px) {
        .action-group {
            width: 100%;
            flex-direction: column;
            border-radius: 16px;
            padding: 12px;
        }

            .action-group .btn {
                width: 100%;
            }
    }

    /* ✅ Modern button system */
    .btn {
        border: none;
        padding: 10px 16px;
        border-radius: 999px;
        cursor: pointer;
        font-weight: 800;
        font-size: 13px;
        transition: transform 0.15s ease, box-shadow 0.15s ease, background 0.15s ease;
        height: 40px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }

        .btn:active {
            transform: translateY(1px);
        }

        .btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

    /* Primary */
    .btn-primary {
        background: #43285d;
        color: #fff;
        box-shadow: 0 8px 18px rgba(67, 40, 93, 0.22);
    }

        .btn-primary:hover {
            box-shadow: 0 10px 24px rgba(67, 40, 93, 0.28);
        }

    /* Secondary */
    .btn-secondary {
        background: #f3f4f6;
        color: #111827;
        border: 1px solid #e5e7eb;
    }

        .btn-secondary:hover {
            background: #eef2f7;
        }

    /* Ghost (for Previous) */
    .btn-ghost {
        background: transparent;
        color: #111827;
        border: 1px solid #e5e7eb;
    }

        .btn-ghost:hover {
            background: #f9fafb;
        }

    /* Errors */
    .error {
        border-color: #dc2626 !important;
    }

    .error-text {
        margin-top: 6px;
        font-size: 12px;
        color: #dc2626;
    }

    /* Responsive */
    @media (max-width: 1100px) {
        .stepper {
            grid-template-columns: repeat(3, minmax(0, 1fr));
            row-gap: 10px;
        }

        .phone-row {
            grid-template-columns: 1fr 1fr;
        }

            .phone-row .field:last-child {
                grid-column: span 2;
            }
    }

    @media (max-width: 900px) {
        .apply-wrap {
            padding: 10px;
        }

        .body,
        .footer,
        .header {
            padding-left: 14px;
            padding-right: 14px;
        }

        .form-grid {
            grid-template-columns: 1fr;
        }

        .span-2 {
            grid-column: span 1;
        }

        .phone-row {
            grid-template-columns: 1fr;
        }

        .stepper {
            grid-template-columns: repeat(2, minmax(0, 1fr));
        }
    }
    .footer-3col {
        padding: 14px 22px;
        border-top: 1px solid #e5e7eb;
        background: #fff;
        display: grid;
        grid-template-columns: 1fr auto 1fr;
        align-items: center;
        gap: 12px;
    }

    .footer-slot.left {
        justify-self: start;
    }

    .footer-slot.center {
        justify-self: center;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
    }

    .footer-slot.right {
        justify-self: end;
        display: flex;
        gap: 10px;
    }

    @media (max-width: 720px) {
        .footer-3col {
            grid-template-columns: 1fr;
            justify-items: stretch;
        }

        .footer-slot.left,
        .footer-slot.right {
            justify-self: stretch;
        }

        .footer-slot.right {
            justify-content: stretch;
        }

            .footer-slot.right .btn,
            .footer-slot.left .btn,
            .footer-slot.center .btn {
                width: 100%;
            }
    }
    .footer-dock {
        padding: 16px 22px;
        border-top: 1px solid #e5e7eb;
        background: #fff;
        display: flex;
        justify-content: center;
    }

    .dock {
        display: flex;
        align-items: center;
        gap: 18px;
        padding: 12px 14px;
        border-radius: 16px;
        background: #f9fafb;
        border: 1px solid #e5e7eb;
        box-shadow: 0 12px 22px rgba(15, 23, 42, 0.06);
    }

    .dock-meta {
        font-size: 12px;
        color: #6b7280;
        padding: 0 10px;
        border-right: 1px solid #e5e7eb;
        white-space: nowrap;
    }

    .dock-actions {
        display: inline-flex;
        align-items: center;
        gap: 16px; /* ✅ more spacing between buttons */
        padding-left: 4px;
    }

        /* ✅ make buttons feel consistent and easier to click */
        .dock-actions .btn {
            height: 44px;
            padding: 10px 22px;
            min-width: 140px; /* ✅ avoids “tiny Save” look */
        }

    /* responsive: stack nicely */
    @media (max-width: 760px) {
        .dock {
            width: 100%;
            flex-direction: column;
            align-items: stretch;
            gap: 10px;
        }

        .dock-meta {
            border-right: none;
            border-bottom: 1px solid #e5e7eb;
            padding: 6px 10px 10px;
            text-align: center;
        }

        .dock-actions {
            flex-direction: column;
            width: 100%;
            gap: 12px;
            padding-left: 0;
        }

            .dock-actions .btn {
                width: 100%;
                min-width: 0;
            }
    }
    .le-card {
        border: 1px solid #e5e7eb;
        background: #fff;
        border-radius: 14px;
        padding: 14px 16px;
        margin-top: 12px;
    }

    .le-card-tight {
        margin-top: 14px;
    }

    .le-text {
        margin: 0 0 10px;
        color: #374151;
        font-size: 13px;
        line-height: 1.45;
    }

    .le-note {
        margin: 0;
        color: #6b7280;
        font-size: 13px;
    }

    .le-grid {
        display: grid;
        grid-template-columns: 1fr;
        gap: 14px;
        margin-top: 14px;
    }

    .le-q {
        border: 1px solid #e5e7eb;
        background: #fafafa;
        border-radius: 14px;
        padding: 14px 16px;
    }

    .le-label {
        display: block;
        font-size: 13px;
        color: #111827;
        margin-bottom: 10px;
        line-height: 1.35;
    }

    .le-textarea {
        width: 100%;
        border: 1px solid #d1d5db;
        border-radius: 12px;
        padding: 12px;
        outline: none;
        background: #fff;
        resize: vertical;
        min-height: 120px;
        transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }

        .le-textarea:focus {
            border-color: rgba(67, 40, 93, 0.6);
            box-shadow: 0 0 0 4px rgba(67, 40, 93, 0.12);
        }

    .le-meta {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 8px;
        gap: 12px;
        font-size: 12px;
        color: #6b7280;
    }

    .le-bad {
        color: #b91c1c;
        font-weight: 700;
    }

    .le-check {
        display: flex;
        gap: 10px;
        align-items: flex-start;
    }

        .le-check input {
            margin-top: 3px;
        }

    .le-h4 {
        margin: 0 0 8px;
        font-size: 14px;
        font-weight: 900;
        color: #111827;
    }

    .le-reminder {
        margin: 0;
        font-size: 12.5px;
        color: #b91c1c;
    }
    .le-title {
        font-size: 20px;
        font-weight: 900;
        margin: 6px 0 14px;
        color: #111827;
    }

    .le-card {
        border: 1px solid #e5e7eb;
        background: #fff;
        border-radius: 14px;
        padding: 16px 18px;
        margin-top: 12px;
    }

    .le-text {
        margin: 0 0 12px;
        color: #111827;
        font-size: 16px; /* ✅ bigger */
        line-height: 1.65; /* ✅ easier reading */
    }

    .le-note {
        margin: 0;
        color: #111827;
        font-size: 16px;
    }

    .le-grid {
        display: grid;
        grid-template-columns: 1fr;
        gap: 16px;
        margin-top: 16px;
    }

    .le-q {
        border: 1px solid #e5e7eb;
        background: #fafafa;
        border-radius: 14px;
        padding: 16px 18px;
    }

    .le-label {
        display: block;
        font-size: 16px; /* ✅ bigger */
        font-weight: 800;
        color: #111827;
        margin-bottom: 10px;
        line-height: 1.45;
    }

    .req {
        color: #b91c1c;
        margin-right: 6px;
        font-weight: 900;
    }

    .le-textarea {
        width: 100%;
        border: 2px solid #d1d5db; /* ✅ thicker border for clarity */
        border-radius: 12px;
        padding: 14px 14px;
        outline: none;
        background: #fff;
        resize: vertical;
        min-height: 150px;
        font-size: 16px; /* ✅ bigger */
        line-height: 1.6;
    }

        .le-textarea:focus {
            border-color: rgba(67, 40, 93, 0.7);
            box-shadow: 0 0 0 5px rgba(67, 40, 93, 0.14);
        }

    .le-meta {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 10px;
        gap: 12px;
        font-size: 14px; /* ✅ bigger */
        color: #374151;
    }

    .le-bad {
        color: #b91c1c;
        font-weight: 900;
    }

    .le-h4 {
        margin: 0 0 10px;
        font-size: 16px;
        font-weight: 900;
        color: #111827;
    }

    .le-reminder {
        margin: 0 0 10px;
        font-size: 15px;
        color: #b91c1c;
        font-weight: 800;
    }

    .le-info {
        margin: 0;
        font-size: 15px;
        color: #111827;
        background: #f3f4f6;
        border: 1px solid #e5e7eb;
        padding: 12px 14px;
        border-radius: 12px;
    }

    .le-alert {
        border: 1px solid #fecaca;
        background: #fef2f2;
        color: #7f1d1d;
        padding: 12px 14px;
        border-radius: 12px;
        font-size: 15px;
        font-weight: 800;
        margin-bottom: 12px;
    }
    .sv-title {
        font-size: 20px;
        font-weight: 900;
        margin: 6px 0 12px;
    }

    .sv-note {
        margin: 0 0 12px;
        background: #f3f4f6;
        border: 1px solid #e5e7eb;
        padding: 12px 14px;
        border-radius: 12px;
        font-size: 14px;
    }

    .sv-grid {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 12px 14px;
    }

    .sv-check {
        display: grid;
        gap: 10px;
        padding-top: 6px;
    }

    .sv-checkline {
        display: flex;
        gap: 10px;
        align-items: center;
        font-weight: 700;
        color: #111827;
    }

    .sv-dates {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 12px 14px;
    }

    @media (max-width: 900px) {
        .sv-grid {
            grid-template-columns: 1fr;
        }

        .sv-dates {
            grid-template-columns: 1fr;
        }
    }
    .upload-grid {
        display: grid;
        grid-template-columns: repeat(2,minmax(0,1fr));
        gap: 14px;
        margin-top: 12px;
    }

    .upload-card {
        border: 1px solid #e5e7eb;
        border-radius: 14px;
        padding: 14px 16px;
        background: #fff;
    }

    .upload-title {
        font-weight: 900;
        margin-bottom: 10px;
    }

    .upload-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        padding: 10px 0;
        border-top: 1px solid #f1f5f9;
    }

    .file-name {
        font-size: 13px;
        color: #111827;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        max-width: 380px;
    }

    .row-actions {
        display: flex;
        gap: 10px;
    }

    @media(max-width:900px) {
        .upload-grid {
            grid-template-columns: 1fr;
        }

        .file-name {
            max-width: 240px;
        }
    }
    .upload-sections {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 14px;
        margin-top: 12px;
    }

    .u-section {
        border: 1px solid #e5e7eb;
        border-radius: 14px;
        background: #fff;
        overflow: hidden;
    }

    .u-section-title {
        padding: 12px 14px;
        font-weight: 900;
        background: #fafafa;
        border-bottom: 1px solid #e5e7eb;
    }

    .u-list {
        padding: 6px 14px 12px;
    }

    .u-row {
        display: grid;
        grid-template-columns: 1fr auto;
        gap: 12px;
        padding: 12px 0;
        border-bottom: 1px solid #f1f5f9;
    }

        .u-row:last-child {
            border-bottom: none;
        }

    .u-name {
        font-weight: 900;
        color: #111827;
    }

    .u-desc {
        margin-top: 4px;
    }

    .u-status {
        margin-top: 6px;
        font-size: 12.5px;
        display: flex;
        gap: 8px;
        align-items: center;
    }

    .u-ok {
        color: #065f46;
        font-weight: 900;
    }

    .u-bad {
        color: #b91c1c;
        font-weight: 900;
    }

    .u-actions {
        display: flex;
        align-items: center;
    }

    .u-upload {
        display: flex;
        gap: 10px;
        align-items: center;
    }

    .u-file {
        display: none; /* hide ugly native input */
    }

    .u-files {
        margin: 2px 0 10px;
        padding: 0 2px;
    }

    .u-file-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        padding: 8px 0;
        border-top: 1px dashed #e5e7eb;
    }

    .u-file-name {
        font-size: 13px;
        color: #111827;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        max-width: 420px;
    }

    .u-file-actions {
        display: flex;
        gap: 10px;
    }

    /* Modal */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(15, 23, 42, 0.55);
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 18px;
        z-index: 9999;
    }

    .modal-card {
        width: min(920px, 100%);
        background: #fff;
        border-radius: 16px;
        border: 1px solid #e5e7eb;
        box-shadow: 0 18px 40px rgba(15, 23, 42, 0.25);
        display: flex;
        flex-direction: column;
        max-height: 90vh; /* key */
        overflow: hidden; /* keeps rounded corners */
    }

    .modal-head {
        flex: 0 0 auto;
    }

    .modal-body {
        flex: 1 1 auto; /* key */
        overflow: auto; /* key: body scrolls */
        padding: 14px;
    }

    .ethics-frame {
        width: 100%;
        height: 55vh; /* don’t consume the entire modal */
        border: 1px solid #e5e7eb;
        border-radius: 12px;
    }
    .ethics-controls {
        margin-top: 12px;
    }

    @media (max-width: 900px) {
        .upload-sections {
            grid-template-columns: 1fr;
        }

        .u-file-name {
            max-width: 240px;
        }

        .ethics-frame {
            height: 420px;
        }
    }
    .u-file-left {
        display: flex;
        align-items: center;
        gap: 10px;
        min-width: 0;
    }

    .u-chip {
        flex: 0 0 auto;
        font-size: 12px;
        font-weight: 900;
        padding: 6px 10px;
        border-radius: 999px;
        border: 1px solid #e5e7eb;
        background: #f9fafb;
        color: #111827;
    }
    .exam-title {
        font-size: 20px;
        font-weight: 900;
        margin: 6px 0 14px;
        color: #111827;
    }

    .exam-intro-card {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 18px;
        padding: 18px 20px;
        border-radius: 16px;
        border: 1px solid #e5e7eb;
        background: linear-gradient(180deg, #ffffff 0%, #faf7ff 100%);
        margin-bottom: 16px;
    }

        .exam-intro-card h4 {
            margin: 0 0 8px;
            font-size: 17px;
            font-weight: 900;
            color: #201132;
        }

        .exam-intro-card p {
            margin: 0;
            font-size: 14px;
            line-height: 1.7;
            color: #4b5563;
            max-width: 860px;
        }

    .exam-summary-pill {
        flex: 0 0 auto;
        padding: 10px 16px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.08);
        color: #43285d;
        font-size: 13px;
        font-weight: 800;
        border: 1px solid rgba(67, 40, 93, 0.12);
    }

        .exam-summary-pill.ready {
            background: rgba(6, 95, 70, 0.10);
            color: #065f46;
            border-color: rgba(6, 95, 70, 0.18);
        }

    .exam-loading-card {
        border: 1px solid #e5e7eb;
        border-radius: 16px;
        padding: 28px;
        background: #fff;
        color: #6b7280;
        font-size: 14px;
    }

    .exam-grid {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 16px;
    }

    .exam-card {
        border: 1px solid #e5e7eb;
        border-radius: 18px;
        padding: 18px;
        background: #fff;
        box-shadow: 0 10px 22px rgba(15, 23, 42, 0.05);
        display: flex;
        flex-direction: column;
        gap: 14px;
        transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease;
    }

        .exam-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 16px 30px rgba(15, 23, 42, 0.08);
        }

        .exam-card.completed {
            border-color: rgba(6, 95, 70, 0.20);
            background: linear-gradient(180deg, #ffffff 0%, #f4fbf8 100%);
        }

    .exam-card-top h4 {
        margin: 0 0 8px;
        font-size: 18px;
        line-height: 1.35;
        font-weight: 850;
        color: #201132;
    }

    .exam-desc {
        margin: 0;
        font-size: 14px;
        color: #5b6472;
        line-height: 1.7;
    }

    .exam-badge {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        margin-bottom: 12px;
        padding: 7px 12px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.10);
        color: #43285d;
        font-size: 11px;
        font-weight: 800;
        letter-spacing: 0.04em;
        text-transform: uppercase;
    }

    .exam-card.completed .exam-badge {
        background: rgba(6, 95, 70, 0.10);
        color: #065f46;
    }

    .exam-progress-row {
        display: flex;
        flex-direction: column;
        gap: 8px;
    }

    .exam-progress-meta {
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 13px;
        color: #4b5563;
    }

        .exam-progress-meta strong {
            color: #111827;
            font-size: 14px;
        }

    .exam-progress-bar {
        width: 100%;
        height: 10px;
        border-radius: 999px;
        background: #edf1f5;
        overflow: hidden;
    }

    .exam-progress-fill {
        height: 100%;
        border-radius: 999px;
        background: linear-gradient(90deg, #43285d 0%, #7c3aed 100%);
        transition: width 0.35s ease;
    }

    .exam-card.completed .exam-progress-fill {
        background: linear-gradient(90deg, #047857 0%, #10b981 100%);
    }

    .exam-card-footer {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        margin-top: auto;
    }

    .exam-status-text {
        font-size: 13px;
        font-weight: 700;
        color: #b91c1c;
    }

        .exam-status-text.ok {
            color: #065f46;
        }

    .exam-player-shell {
        border: 1px solid #e5e7eb;
        border-radius: 18px;
        overflow: hidden;
        background: #fff;
        box-shadow: 0 14px 30px rgba(15, 23, 42, 0.07);
    }

    .exam-player-header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        padding: 16px 18px;
        border-bottom: 1px solid #e5e7eb;
        background: #fafafa;
    }

        .exam-player-header h4 {
            margin: 0 0 4px;
            font-size: 18px;
            font-weight: 850;
            color: #201132;
        }

        .exam-player-header p {
            margin: 0;
            color: #6b7280;
            font-size: 13px;
        }

    .exam-bottom-note {
        margin-top: 16px;
        padding: 14px 16px;
        border-radius: 14px;
        border: 1px solid #f3d2d2;
        background: #fff7f7;
        color: #7f1d1d;
        font-size: 14px;
    }

        .exam-bottom-note.ready {
            border-color: #cce9dc;
            background: #f3fbf7;
            color: #065f46;
        }

    @media (max-width: 1000px) {
        .exam-grid {
            grid-template-columns: 1fr;
        }
    }

    @media (max-width: 760px) {
        .exam-intro-card,
        .exam-player-header,
        .exam-card-footer {
            flex-direction: column;
            align-items: stretch;
        }

        .exam-summary-pill {
            width: fit-content;
        }
    }
    .submit-overlay {
        background: rgba(15, 23, 42, 0.62);
        backdrop-filter: blur(4px);
    }

    .submit-success-modal {
        width: min(560px, 92vw);
        max-height: unset;
        overflow: hidden;
        border: none;
        border-radius: 24px;
        box-shadow: 0 28px 60px rgba(15, 23, 42, 0.28);
        background: linear-gradient(180deg, #ffffff 0%, #faf8ff 100%);
        animation: submitPopIn 0.22s ease-out;
    }

    .submit-success-top {
        display: flex;
        align-items: center;
        gap: 16px;
        padding: 24px 24px 14px;
        border-bottom: 1px solid #ece7f3;
    }

    .submit-success-icon {
        width: 64px;
        height: 64px;
        min-width: 64px;
        border-radius: 999px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 30px;
        font-weight: 900;
        color: #ffffff;
        background: linear-gradient(135deg, #16a34a 0%, #22c55e 100%);
        box-shadow: 0 14px 28px rgba(34, 197, 94, 0.28);
    }

    .submit-success-title {
        margin: 0;
        font-size: 24px;
        line-height: 1.2;
        font-weight: 900;
        color: #1f1630;
    }

    .submit-success-subtitle {
        margin: 6px 0 0;
        font-size: 14px;
        color: #6b7280;
        line-height: 1.5;
    }

    .submit-success-body {
        padding: 20px 24px 24px;
        overflow: visible;
    }

    .success-text {
        margin: 0;
        font-size: 16px;
        line-height: 1.75;
        color: #374151;
        background: #f8fafc;
        border: 1px solid #e5e7eb;
        border-left: 5px solid #43285d;
        border-radius: 16px;
        padding: 16px 18px;
    }

    .submit-success-actions {
        display: flex;
        justify-content: flex-end;
        margin-top: 20px;
    }

    .submit-ok-btn {
        min-width: 120px;
        height: 46px;
        font-size: 14px;
        padding: 0 24px;
        border-radius: 999px;
        box-shadow: 0 12px 24px rgba(67, 40, 93, 0.24);
    }

    @keyframes submitPopIn {
        from {
            opacity: 0;
            transform: translateY(14px) scale(0.96);
        }

        to {
            opacity: 1;
            transform: translateY(0) scale(1);
        }
    }

    @media (max-width: 640px) {
        .submit-success-modal {
            width: min(94vw, 94vw);
            border-radius: 20px;
        }

        .submit-success-top {
            align-items: flex-start;
            padding: 20px 18px 12px;
        }

        .submit-success-icon {
            width: 54px;
            height: 54px;
            min-width: 54px;
            font-size: 24px;
        }

        .submit-success-title {
            font-size: 20px;
        }

        .submit-success-subtitle {
            font-size: 13px;
        }

        .submit-success-body {
            padding: 16px 18px 20px;
        }

        .success-text {
            font-size: 15px;
            padding: 14px 15px;
        }

        .submit-success-actions {
            justify-content: stretch;
        }

        .submit-ok-btn {
            width: 100%;
            min-width: 0;
        }
    }
    .track-multi {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 12px;
        border: 1px solid #d1d5db;
        border-radius: 14px;
        padding: 14px;
        background: #fff;
    }

    .track-option {
        display: flex;
        align-items: center;
        gap: 10px;
        padding: 12px 14px;
        border: 1px solid #e5e7eb;
        border-radius: 12px;
        background: #fafafa;
        font-size: 14px;
        font-weight: 700;
        color: #111827;
        cursor: pointer;
    }

        .track-option input {
            width: 16px;
            height: 16px;
            accent-color: #43285d;
        }

    .track-multi.error {
        border-color: #dc2626;
    }

    @media (max-width: 900px) {
        .track-multi {
            grid-template-columns: 1fr;
        }
    }
    .track-option.disabled {
        opacity: 0.55;
        cursor: not-allowed;
    }

        .track-option.disabled input,
        .track-option.disabled span {
            cursor: not-allowed;
        }
</style>