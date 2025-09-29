<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal details">
            <header class="modal-header">
                <h3>📘 Course Details</h3>
                <button class="icon-close danger" @click="$emit('close')" aria-label="Close">✖</button>
            </header>

            <!-- Title row (separate, big, single line) + tags with counts -->
            <section class="title-and-tags">
                <h2 class="course-title-one-line" :title="courseInfo.subjectTitle">
                    {{ courseInfo.subjectTitle || '—' }}
                </h2>
                <div class="tag-row">
                    <span v-if="counts.waitlistCount > 0" class="tag tag-amber">
                        ⏳ Waitlist ({{ counts.waitlistCount }})
                    </span>
                    <span v-if="adaCount > 0" class="tag tag-green" @click="openAdaFromTag" title="Show ADA details">
                        ♿ ADA ({{ adaCount }})
                    </span>
                    <span v-if="courseInfo.cancelled" class="tag tag-gray">🚫 Cancelled</span>
                </div>
            </section>

            <!-- same compact 2-col layout -->
            <section class="grid-two">
                <div>
                    <div class="pair">
                        <span class="label">Training Center</span>
                        <span class="value">{{ courseInfo.siteName || '—' }}</span>
                    </div>

                    <div class="pair">
                        <span class="label">Location</span>
                        <span class="value">{{ courseInfo.trainingLocation || '—' }}</span>
                    </div>

                    <div class="pair">
                        <span class="label">Region</span>
                        <span class="value">{{ courseInfo.regionLabel || '—' }}</span>
                    </div>

                    <div class="pair">
                        <span class="label">Category</span>
                        <span class="value">{{ courseInfo.categoryLabel || '—' }}</span>
                    </div>

                    <div class="pair">
                        <span class="label">Format</span>
                        <span class="value">{{ formatCodeLabel(courseInfo.format) }}</span>
                    </div>
                </div>

                <div>
                    <div class="pair">
                        <span class="label">Start Date</span>
                        <span class="value">{{ fmtDate(courseInfo.courseDate) }}</span>
                    </div>

                    <div class="pair" v-if="courseInfo.endDate">
                        <span class="label">End Date</span>
                        <span class="value">{{ fmtDate(courseInfo.endDate) }}</span>
                    </div>

                    <div class="pair">
                        <span class="label">Time</span>
                        <span class="value">{{ fmtTime(courseInfo.courseTimeBegin) }} – {{ fmtTime(courseInfo.courseTimeEnd) }}</span>
                    </div>

                    <div class="pair">
                        <span class="label">Registration Deadline</span>
                        <span class="value">{{ fmtDate(courseInfo.regDeadLine) || '—' }}</span>
                    </div>

                    <div class="pair">
                        <span class="label">Delivered</span>
                        <span class="value">{{ deliveredText }}</span>
                    </div>

                    <div class="pair">
                        <span class="label">Approval</span>
                        <span class="value"><StatusPill :state="approvalState" /></span>
                    </div>
                </div>
            </section>

            <!-- capacity -->
            <section class="capacity">
                <div class="capacity-item">
                    <div class="cap-label">Total Sign-ups</div>
                    <div class="cap-value">
                        <strong>{{ counts.totalRegistrations }}</strong>
                        <span v-if="Number.isFinite(counts.maxSeats)">/ {{ counts.maxSeats }}</span>
                    </div>
                </div>
                <div class="capacity-item">
                    <div class="cap-label">Enrolled (not WL)</div>
                    <div class="cap-value">{{ counts.enrolledCount }}</div>
                </div>
                <div class="capacity-item">
                    <div class="cap-label">Waitlist</div>
                    <div class="cap-value">{{ counts.waitlistCount }}</div>
                </div>
            </section>

            <!-- ADA (click to reveal registrations + messages) -->
            <section class="notes">
                <h4>♿ ADA Details</h4>

                <div class="ada-row">
                    <button class="btn-primary btn-xs" @click="toggleAda">
                        {{ adaOpen ? 'Hide ADA Details' : 'Show ADA Details' }}
                        <span v-if="adaCount">({{ adaCount }})</span>
                    </button>

                    <!-- NEW: Download CSV -->
                    <button class="btn-secondary btn-xs"
                            :disabled="adaLoading || (!adaOpen && adaCount === 0) || (adaOpen && adaList.length === 0)"
                            title="Download ADA registrations as CSV"
                            @click="downloadAdaCsv">
                        ⬇️ Download CSV
                    </button>
                </div>

                <transition name="fade">
                    <div v-if="adaOpen">
                        <div v-if="adaLoading" class="presenter-note">Loading ADA registrations…</div>
                        <div v-else-if="!adaList.length" class="presenter-note">No ADA requests found.</div>
                        <div v-else class="ada-table-wrap">
                            <table class="session-table">
                                <thead>
                                    <tr>
                                        <th style="width:220px">Name</th>
                                        <th style="width:260px">Email</th>
                                        <th>ADA Message</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="r in adaList" :key="r.userSysId">
                                        <td>{{ r.fullName }}</td>
                                        <td><a :href="`mailto:${r.email}`">{{ r.email }}</a></td>
                                        <td><span class="mono" style="white-space:pre-wrap">{{ r.adaDetails || 'Requested ADA assistance' }}</span></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </transition>
            </section>

            <!-- sessions -->
            <section class="sessions" v-if="sessions.length">
                <h4>🗓 Sessions</h4>
                <table class="session-table">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Start</th>
                            <th>End</th>
                            <th>Link</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(s, i) in sessions" :key="i">
                            <td>{{ fmtDate(s.sessionDate) }}</td>
                            <td>{{ fmtTime(s.startTime) }}</td>
                            <td>{{ fmtTime(s.endTime) }}</td>
                            <td>
                                <a v-if="s.sessionUrl" :href="s.sessionUrl" target="_blank" rel="noopener">Open</a>
                                <span v-else>—</span>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </section>

            <!-- notes -->
            <section class="notes" v-if="courseInfo.information || courseInfo.deliverable">
                <h4>📝 Notes</h4>
                <div class="note-block" v-if="courseInfo.information">
                    <div class="note-title">Information</div>
                    <div class="note-text mono">{{ courseInfo.information }}</div>
                </div>
                <div class="note-block" v-if="courseInfo.deliverable">
                    <div class="note-title">Deliverable</div>
                    <div class="note-text mono">{{ courseInfo.deliverable }}</div>
                </div>
            </section>

            <!-- instructors LAST -->
            <section class="instructors">
                <h4>👩‍🏫 Instructors</h4>
                <ul>
                    <li v-if="courseInfo.instructorLabel">{{ courseInfo.instructorLabel }}</li>
                    <li v-if="courseInfo.instructor2Label">{{ courseInfo.instructor2Label }}</li>
                    <li v-if="!courseInfo.instructorLabel && !courseInfo.instructor2Label">—</li>
                </ul>
            </section>

            <!-- bottom centered red close -->
            <footer class="modal-footer center">
                <button class="btn-danger" @click="$emit('close')">Close</button>
            </footer>
        </div>
    </div>
</template>
<script>
import apiClient from "@/axios.js";

const StatusPill = {
  props: { state: String },
  template: `<span :class="['pill', state]"><slot>{{ text }}</slot></span>`,
  computed: {
    text() {
      if (this.state === 'yes') return 'Yes';
      if (this.state === 'no') return 'No';
      return 'Pending';
    }
  }
};

export default {
  name: "ViewCourseDetailsModal",
  components: { StatusPill },
  props: { course: { type: Object, required: true } },
  data() {
    return {
      courseInfo: { ...this.course },
      sessions: [],
      counts: {
        enrolledCount: 0,
        waitlistCount: 0,
        totalRegistrations: 0,
        hasWaitlist: false,
        hasAda: false,
        maxSeats: null
      },
      adaOpen: false,
      adaLoading: false,
      adaList: [],
      adaCount: 0
    };
  },
  computed: {
    approvalState() {
      const v = this.courseInfo.approve;
      if (v === true) return 'yes';
      if (v === false) return 'no';
      return 'pending';
    },
    deliveredText() {
      const endOrStart = this.courseInfo.endDate || this.courseInfo.courseDate;
      const isPast = endOrStart ? new Date(endOrStart) < new Date() : false;
      return this.courseInfo.delivered === true || isPast ? 'Yes' : 'No';
    }
  },
  mounted() {
    this.loadDetails();
  },
  methods: {
    async loadDetails() {
      const id = this.courseInfo.courseSysId;
      try {
        const { data } = await apiClient.get(`/CourseAdmin/courseWithSessions/${id}`);
        this.courseInfo = {
          ...this.courseInfo,
          ...data,
          subjectTitle: this.courseInfo.subjectTitle ?? data.subjectTitle,
          siteName: this.courseInfo.siteName ?? data.siteName,
          regionLabel: this.courseInfo.regionLabel,
          categoryLabel: this.courseInfo.categoryLabel,
          instructorLabel: this.courseInfo.instructorLabel,
          instructor2Label: this.courseInfo.instructor2Label
        };
        this.sessions = Array.isArray(data.sessions) ? data.sessions : (data.sessions?.$values ?? []);

        const c = await apiClient.get('/CourseAdmin/counts', { params: { courseId: id } });
        const payload = c.data || {};
        const enrolled = Number.isFinite(payload.enrolledCount) ? payload.enrolledCount : 0;
        const wl = Number.isFinite(payload.waitlistCount) ? payload.waitlistCount : 0;

        this.counts.enrolledCount = enrolled;
        this.counts.waitlistCount = wl;
        this.counts.totalRegistrations = Number.isFinite(payload.totalRegistrations) ? payload.totalRegistrations : (enrolled + wl);
        this.counts.hasWaitlist = !!payload.hasWaitlist || wl > 0;
        this.counts.maxSeats = (typeof payload.maxSeats === 'number' || payload.maxSeats === null)
          ? payload.maxSeats
          : this.courseInfo.maxSeats ?? null;

        // Load ADA COUNT
        try {
          const adaRes = await apiClient.get('/CourseAdmin/ada-registrations', { params: { courseId: id } });
          const arr = Array.isArray(adaRes.data) ? adaRes.data : (adaRes.data?.$values ?? []);
          this.adaCount = arr.length;
          this.counts.hasAda = this.adaCount > 0;
        } catch { /* ignore count error */ }
      } catch (e) {
        console.error("❌ Failed to load course details:", e);
      }
    },

    async toggleAda() {
      this.adaOpen = !this.adaOpen;
      if (!this.adaOpen) return;
      if (this.adaList.length) return;

      await this.ensureAdaListLoaded();
    },

    openAdaFromTag() {
      if (!this.adaOpen) this.toggleAda();
      else window.requestAnimationFrame(() => this.toggleAda());
    },

    async downloadAdaCsv() {
      // Ensure we have data (fetch if panel is closed or list is empty)
      if (!this.adaOpen || !this.adaList.length) {
        await this.ensureAdaListLoaded();
      }
      if (!this.adaList.length) return;

      const rows = [
        ["Name", "Email", "ADA Need", "ADA Details"],
        ...this.adaList.map(r => [
          r.fullName || "",
          r.email || "",
          r.adaNeed ? "Yes" : "No",
          r.adaDetails || ""
        ])
      ];

      const csv = rows
        .map(cols =>
          cols
            .map(v => {
              const s = String(v ?? "");
              if (/[",\n]/.test(s)) return `"${s.replace(/"/g, '""')}"`;
              return s;
            })
            .join(",")
        )
        .join("\n");

      const blob = new Blob(["\uFEFF" + csv], { type: "text/csv;charset=utf-8;" });

      const a = document.createElement("a");
      const url = URL.createObjectURL(blob);
      a.href = url;

      const slug = this.slugify(this.courseInfo.subjectTitle || "course");
      a.download = `ada-registrations_${slug}_${this.courseInfo.courseSysId}.csv`;

      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    },

    async ensureAdaListLoaded() {
      try {
        if (this.adaLoading) return;
        this.adaLoading = true;
        const id = this.courseInfo.courseSysId;
        const { data } = await apiClient.get('/CourseAdmin/ada-registrations', { params: { courseId: id } });
        const list = Array.isArray(data) ? data : (data?.$values ?? []);
        this.adaList = list.map(x => ({
          userSysId: x.userSysId ?? x.UserSysId,
          fullName: x.fullName ?? x.FullName,
          email: x.email ?? x.Email,
          adaNeed: !!(x.adaNeed ?? x.Adaneed),
          adaDetails: (x.adaDetails ?? x.Adadetails ?? '').trim()
        }));
      } catch (e) {
        console.error("❌ Failed to load ADA registrations:", e);
        this.adaList = [];
      } finally {
        this.adaLoading = false;
      }
    },

    slugify(s) {
      return String(s)
        .toLowerCase()
        .replace(/\s+/g, "-")
        .replace(/[^a-z0-9-]/g, "")     
        .replace(/-+/g, "-")           
        .replace(/^-+|-+$/g, "");       
    },

    fmtDate(v) {
      if (!v) return "";
      const d = new Date(v);
      return isNaN(d) ? "" : d.toLocaleDateString("en-US");
    },
    fmtTime(v) {
      if (!v) return "—";
      const s = String(v).slice(0, 5);
      return s || "—";
    },
    formatCodeLabel(code) {
      return typeof code === 'number' ? `#${code}` : (code || '—');
    }
  }
};
</script>

<style scoped>
    /* Overlay & shell */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,.65);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 9999;
    }

    .modal.details {
        background: #fff;
        width: 900px;
        max-height: 90vh;
        overflow-y: auto;
        border-radius: 16px;
        padding: 24px 28px;
        box-shadow: 0 18px 40px rgba(0,0,0,.2);
    }

    .modal-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 12px;
    }

    .icon-close {
        border: none;
        background: #f5f5f5;
        border-radius: 8px;
        padding: 6px 10px;
        cursor: pointer;
    }

        .icon-close.danger {
            background: #ffe7e7;
            color: #b71c1c;
            border: 1px solid #ffc9c9;
        }

            .icon-close.danger:hover {
                background: #ffd7d7;
            }

    /* Title + tags */
    .title-and-tags {
        display: flex;
        align-items: center;
        gap: 16px;
        margin: 4px 0 10px;
        flex-wrap: wrap;
    }

    .course-title-one-line {
        font-size: 24px;
        font-weight: 700;
        margin: 0;
        flex: 1 1 520px;
        min-width: 280px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        line-height: 1.25;
    }

    .tag-row {
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
    }

    .tag {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        padding: 4px 10px;
        font-size: 12px;
        font-weight: 700;
        border-radius: 999px;
        border: 1px solid rgba(0,0,0,0.06);
        cursor: default;
        user-select: none;
    }

    .tag-amber {
        color: #7a3e00;
        background: linear-gradient(180deg,#fff5e6,#ffe8c7);
        border-color: #ffd9a1;
    }

    .tag-green {
        color: #084c2e;
        background: linear-gradient(180deg,#eafff5,#d5f7ea);
        border-color: #bdeedc;
        cursor: pointer;
    }

    .tag-gray {
        color: #555;
        background: #f2f2f2;
        border-color: #e0e0e0;
    }

    /* Info grid */
    .grid-two {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 18px;
        margin-top: 8px;
    }

    .pair {
        display: flex;
        justify-content: space-between;
        gap: 12px;
        padding: 8px 0;
        border-bottom: 1px dashed #eee;
    }

    .label {
        color: #666;
        min-width: 160px;
    }

    .value {
        font-weight: 600;
        min-width: 0;
        word-break: break-word;
    }

    /* Capacity cards */
    .capacity {
        display: grid;
        grid-template-columns: repeat(3,1fr);
        gap: 12px;
        margin: 10px 0 4px;
    }

    .capacity-item {
        background: #fafafa;
        border: 1px solid #eee;
        border-radius: 12px;
        padding: 12px 14px;
    }

    .cap-label {
        font-size: 12px;
        color: #777;
        margin-bottom: 6px;
    }

    .cap-value {
        font-size: 18px;
    }

    /* Sections */
    .instructors, .sessions, .notes {
        margin-top: 16px;
    }

    h4 {
        margin: 12px 0;
    }

    /* Table */
    .session-table {
        width: 100%;
        border-collapse: collapse;
    }

        .session-table th, .session-table td {
            border-bottom: 1px solid #eee;
            padding: 10px;
            text-align: left;
        }

    /* Notes blocks */
    .note-block {
        background: #fafafa;
        border: 1px solid #eee;
        border-radius: 12px;
        padding: 12px 14px;
        margin-top: 8px;
    }

    .note-title {
        font-weight: 600;
        color: #444;
        margin-bottom: 6px;
    }

    .note-text {
        white-space: pre-wrap;
        color: #333;
    }

    /* Status pills */
    .pill {
        padding: 2px 8px;
        border-radius: 999px;
        font-weight: 600;
        font-size: 12px;
    }

        .pill.yes {
            background: #e8f5e9;
            color: #145a32;
        }

        .pill.no {
            background: #fdecea;
            color: #8a1c12;
        }

        .pill.pending {
            background: #fff8e1;
            color: #7a4d00;
        }

    /* Buttons (reusing your other page, plus red close) */
    .modal-footer.center {
        display: flex;
        justify-content: center;
        margin-top: 22px;
    }

    .btn-primary, .btn-secondary, .btn-danger {
        padding: 10px 20px;
        font-size: 16px;
        border-radius: 6px;
        font-weight: 600;
        cursor: pointer;
        transition: background-color .2s ease;
    }

    .btn-primary {
        background: #388e3c;
        color: #fff;
        border: none;
    }

        .btn-primary:hover {
            background: #2e7d32;
        }

    .btn-secondary {
        background: #e0e0e0;
        color: #333;
        border: none;
    }

        .btn-secondary:hover {
            background: #d0d0d0;
        }

    /* Red close button */
    .btn-danger {
        background: #e53935;
        color: #fff;
        border: none;
    }

        .btn-danger:hover {
            background: #c62828;
        }

    /* tiny primary (for ADA toggle) */
    .btn-xs {
        padding: 6px 10px;
        font-size: 14px;
    }

    /* Misc */
    .mono {
        font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', monospace;
    }

    /* Fade */
    .fade-enter-active, .fade-leave-active {
        transition: opacity .18s ease;
    }

    .fade-enter-from, .fade-leave-to {
        opacity: 0;
    }
    .ada-row {
        display: flex;
        align-items: center;
        gap: 8px;
        margin: 6px 0 10px;
    }

    /* Optional: better disabled button visuals */
    button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }
</style>