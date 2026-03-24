<template>
    <div class="peer-page">
        <!-- HERO -->
        <section class="hero">
            <div class="hero-content">
                <div class="hero-badge">New York State Peer Worker Certification</div>

                <h1>NYS Peer Worker Certification Information and Resources</h1>

                <p class="hero-text">
                    The AIDS Institute has long recognized the vital role peer workers play in improving health outcomes.
                    Peer-delivered services support linkage to care, retention in care, viral suppression, prevention,
                    and stronger engagement across health and social service settings.
                </p>

                <div class="hero-actions">
                    <button class="btn btn-primary" @click="goToApply">
                        Apply for Peer Certification
                    </button>

                    <a class="btn btn-secondary" :href="links.faq" target="_blank" rel="noopener">
                        View FAQs
                    </a>

                    <button class="btn btn-outline" @click="openTrackModal">
                        Track My Application
                    </button>
                </div>
            </div>

            <div class="hero-side">
                <div class="stat-card">
                    <div class="stat-number">90</div>
                    <div class="stat-label">Total training hours required</div>
                </div>

                <div class="stat-card">
                    <div class="stat-number">500</div>
                    <div class="stat-label">Supervised practicum hours</div>
                </div>

                <div class="stat-card">
                    <div class="stat-number">75%</div>
                    <div class="stat-label">Minimum online exam passing score</div>
                </div>
            </div>
        </section>

        <!-- QUICK INFO -->
        <section class="section">
            <div class="section-head">
                <h2>Program Overview</h2>
                <p>
                    Explore requirements, training tools, certification resources, and the materials needed to begin
                    your application.
                </p>
            </div>

            <div class="info-grid">
                <article class="info-card">
                    <h3>Certification Tracks</h3>
                    <ul class="clean-list">
                        <li>HIV Peer Worker</li>
                        <li>HCV Peer Worker</li>
                        <li>Harm Reduction Peer Worker</li>
                        <li>PrEP Peer Worker</li>
                        <li>Criminal Justice Peer Worker</li>
                    </ul>
                </article>

                <article class="info-card">
                    <h3>What You Need</h3>
                    <ul class="clean-list">
                        <li>Qualifying lived experience</li>
                        <li>Required training hours</li>
                        <li>Supervised practicum</li>
                        <li>Online exam completion</li>
                        <li>Application review board approval</li>
                    </ul>
                </article>

                <article class="info-card">
                    <h3>Helpful Starting Points</h3>
                    <div class="link-stack">
                        <a :href="links.process" target="_blank" rel="noopener">Process, Requirements, Application</a>
                        <a :href="links.catalog" target="_blank" rel="noopener">Course Catalog</a>
                        <a :href="links.courseTrackers" target="_blank" rel="noopener">Course Tracker Forms</a>
                        <a :href="links.foundational" target="_blank" rel="noopener">Foundational Training</a>
                    </div>
                </article>
            </div>
        </section>

        <!-- UPDATES -->
        <section class="section">
            <div class="section-head">
                <h2>Program Updates</h2>
                <p>Current announcements and recent additions to the certification program.</p>
            </div>

            <div class="update-banner">
                <div class="update-chip">Spring / Summer 2025</div>
                <h3>New courses are available through HIVtrainingny.org</h3>
                <p>
                    The certification program has added new and upcoming offerings, including courses related to the
                    peer worker role, criminal justice reentry, continuity of HIV/HCV/PrEP care, and peer services in
                    correctional settings.
                </p>
            </div>

            <div class="two-col-grid">
                <div class="panel">
                    <h3>Available Now</h3>
                    <ul class="clean-list">
                        <li>Introduction to the Peer Worker Role</li>
                        <li>The Role of the Criminal Justice Peer Worker in Community Reentry</li>
                        <li>Continuity of HIV/HCV/PrEP Care: Transitioning from a Correctional Setting to the Community</li>
                        <li>Understanding the NYS Criminal Justice System</li>
                        <li>Peer Workers Promoting Sexual Health in the Context of Community Reentry</li>
                    </ul>
                </div>

                <div class="panel">
                    <h3>Coming Soon</h3>
                    <ul class="clean-list">
                        <li>Inventory of the Peer Worker</li>
                        <li>Providing Peer Services in a Correctional Facility</li>
                        <li>Addressing Substance Use in the Transition from Correctional Facility to the Community</li>
                    </ul>
                </div>
            </div>

            <div class="support-network">
                <h3>Certified Peer Worker Support Network Meetings</h3>
                <p>
                    Certified Peer Workers can attend support network meetings to hear program updates, learn about new
                    opportunities, and stay connected.
                </p>

                <div class="meeting-list">
                    <div class="meeting-item">May 28th – 2:00 PM to 3:30 PM</div>
                    <div class="meeting-item">June 25th – 7:00 PM to 8:30 PM</div>
                    <div class="meeting-item">July 23rd – 2:00 PM to 3:30 PM</div>
                </div>

                <p class="small-note">
                    For more information, contact
                    <a href="mailto:hivet@ny.health.gov">hivet@ny.health.gov</a>.
                </p>
            </div>
        </section>

        <!-- RESOURCES -->
        <section class="section">
            <div class="section-head">
                <h2>Peer Worker Resources</h2>
                <p>Program documents, study tools, course planning resources, and guidance materials.</p>
            </div>

            <div class="resource-grid">
                <article class="resource-card" v-for="item in peerResources" :key="item.title">
                    <div class="resource-icon">↗</div>
                    <div class="resource-body">
                        <h3>{{ item.title }}</h3>
                        <p>{{ item.desc }}</p>
                        <a :href="item.href" target="_blank" rel="noopener">Open Resource</a>
                    </div>
                </article>
            </div>
        </section>

        <!-- CERTIFIED PEER RESOURCES -->
        <section class="section">
            <div class="section-head">
                <h2>Resources for Certified Peer Workers</h2>
                <p>Maintain certification standards, ethics, and continuing education requirements.</p>
            </div>

            <div class="card-list">
                <a v-for="item in certifiedResources"
                   :key="item.title"
                   class="simple-link-card"
                   :href="item.href"
                   target="_blank"
                   rel="noopener">
                    <div>
                        <h3>{{ item.title }}</h3>
                        <p>{{ item.desc }}</p>
                    </div>
                    <span>Open</span>
                </a>
            </div>
        </section>

        <!-- ORG / SUPERVISOR -->
        <section class="section">
            <div class="section-head">
                <h2>Resources for Organizations and Supervisors</h2>
                <p>Tools to support supervision, organizational readiness, and integration of peer workers.</p>
            </div>

            <div class="card-list">
                <a v-for="item in organizationResources"
                   :key="item.title"
                   class="simple-link-card"
                   :href="item.href"
                   target="_blank"
                   rel="noopener">
                    <div>
                        <h3>{{ item.title }}</h3>
                        <p>{{ item.desc }}</p>
                    </div>
                    <span>Open</span>
                </a>
            </div>
        </section>

        <!-- EMPLOYMENT -->
        <section class="section">
            <div class="section-head">
                <h2>Employment & Benefits</h2>
                <p>Employment support and related guidance for peer workers.</p>
            </div>

            <div class="info-grid">
                <article class="info-card" v-for="item in employmentResources" :key="item.title">
                    <h3>{{ item.title }}</h3>
                    <p>{{ item.desc }}</p>
                    <a :href="item.href" target="_blank" rel="noopener">Learn More</a>
                </article>
            </div>
        </section>

        <!-- ELIGIBILITY -->
        <section class="section">
            <div class="section-head">
                <h2>Who Is Eligible for Certification?</h2>
            </div>

            <div class="eligibility-box">
                <p>
                    Any person with a qualifying lived experience related to HIV, Hepatitis C, Harm Reduction,
                    Pre-Exposure Prophylaxis (PrEP), or Criminal Justice may be eligible for certification.
                </p>

                <p>
                    In addition, applicants must demonstrate work or practicum experience related to peer
                    responsibilities and the competencies connected to their selected certification track.
                </p>
            </div>

            <div class="tracks-grid">
                <div class="track-pill">HIV</div>
                <div class="track-pill">HCV</div>
                <div class="track-pill">Harm Reduction</div>
                <div class="track-pill">PrEP</div>
                <div class="track-pill">Criminal Justice</div>
            </div>
        </section>

        <!-- CONFIDENTIALITY / APPLY -->
        <section class="cta-section">
            <div class="cta-card">
                <div class="cta-copy">
                    <div class="cta-badge">Before You Apply</div>
                    <h2>Review the confidentiality and lived experience notice</h2>
                    <p>
                        By starting the application, you acknowledge that selecting a certification track and completing
                        the statement of lived experience may involve sharing personal information. The NYS DOH states
                        that applications are reviewed confidentially by authorized staff and the review board.
                    </p>

                    <p class="small-note">
                        Prior to starting your application, please review the
                        <a :href="links.faq" target="_blank" rel="noopener">peer certification FAQ</a>.
                    </p>
                </div>

                <div class="cta-actions">
                    <button class="btn btn-primary large-btn" @click="goToApply">
                        Apply for Peer Certification
                    </button>

                    <a class="btn btn-secondary large-btn" :href="links.faq" target="_blank" rel="noopener">
                        Review FAQs First
                    </a>
                </div>
            </div>
        </section>
        <div v-if="showTrackModal" class="modal-overlay" @click.self="closeTrackModal">
            <div class="track-modal" role="dialog" aria-modal="true" aria-label="Track My Application">
                <div class="track-modal-header">
                    <div>
                        <div class="track-modal-badge">Application Tracker</div>
                        <h3>Track My Peer Certification Applications</h3>
                        <p>
                            View all of your peer certification application records, including status,
                            dates, certification track, and completion progress.
                        </p>
                    </div>

                    <button class="modal-close-btn" @click="closeTrackModal">✕</button>
                </div>

                <div class="track-modal-body">
                    <div v-if="trackingLoading" class="track-state-card">
                        Loading your application details...
                    </div>

                    <div v-else-if="trackingError" class="track-state-card track-error-card">
                        {{ trackingError }}
                    </div>

                    <div v-else-if="applicationTracks.length === 0" class="track-state-card">
                        No peer certification applications were found for your account.
                    </div>

                    <div v-else class="track-table-wrap">
                        <div class="track-help-note">
                            <strong>Note:</strong>
                            Pending means your application was submitted and is waiting for review.
                            Approved means your application has been accepted.
                            Disapproved means your application was reviewed but not approved.
                        </div>

                        <div class="track-table-scroll">
                            <table class="track-table multi-track-table">
                                <thead>
                                    <tr>
                                        <th>Peer ID</th>
                                        <th>Track</th>
                                        <th>Status</th>
                                        <th>Active</th>
                                        <th>Submitted On</th>
                                        <th>Last Updated</th>
                                        <th>Required Courses</th>
                                        <th>Self-Care</th>
                                        <th>Practicum</th>
                                        <th>500 Hours</th>
                                        <th>Uploads</th>
                                        <th>Exams</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    <tr v-for="row in applicationTracks" :key="row.peerSysId">
                                        <td>{{ row.peerSysId }}</td>
                                        <td>{{ row.certificationTrack || "—" }}</td>
                                        <td>
                                            <span class="status-pill" :class="statusClass(row.applicationStatus)">
                                                {{ row.applicationStatus }}
                                            </span>
                                        </td>
                                        <td>
                                            <span :class="row.active ? 'cell-good' : 'cell-muted'">
                                                {{ row.active ? "Yes" : "No" }}
                                            </span>
                                        </td>
                                        <td>{{ formatDate(row.submittedOn) }}</td>
                                        <td>{{ formatDate(row.lastUpdated) }}</td>
                                        <td>{{ yesNo(row.requiredCoursesConfirmed) }}</td>
                                        <td>{{ yesNo(row.selfCareCompleted) }}</td>
                                        <td>{{ yesNo(row.practicumCompleted) }}</td>
                                        <td>{{ yesNo(row.practicumMin500) }}</td>
                                        <td>
                                            {{ row.uploadedRequiredDocs }}/{{ row.totalRequiredDocs }}
                                        </td>
                                        <td>
                                            {{ row.completedExamCount }}/{{ row.totalExamCount }}
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                <div class="track-modal-footer">
                    <button class="btn btn-secondary" @click="closeTrackModal">
                        Close
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
<script>export default {
        name: "PeerCertificationPage",
        data() {
            return {
                showTrackModal: false,
                trackingLoading: false,
                trackingError: "",
                applicationTracks: [],
                links: {
                    faq: "https://www.hivtrainingny.org/FAQDocs/PeerCertificationFAQ2.pdf",
                    process: "https://www.hivtrainingny.org/User/ConfirmCourse/3627",
                    courseTrackers: "https://www.hivtrainingny.org/Home/PeerCheckList",
                    catalog: "https://www.hivtrainingny.org/FAQDocs/PeerCertificationCourseCatalogue2.pdf",
                    selfCare: "https://www.hivtrainingny.org/FAQDocs/PeerCertificationReadinessSelfAssessment.pdf",
                    suicide: "https://www.hivtrainingny.org/Uploads/LivingWorksStartGuidance091422.pdf",
                    supervisorEval: "https://www.hivtrainingny.org/Home/PeerExam",
                    competencies: "https://www.hivtrainingny.org/PeerCert/CoreCompetency",
                    studyGuides: "https://www.hivtrainingny.org/PeerCert/StudyGuide",
                    ceus: "https://www.hivtrainingny.org/FAQDocs/CEUGuidance_FAQ.pdf",
                    ethics: "https://www.hivtrainingny.org/PeerCert/CodeOfEthics",
                    ethicsViolation: "https://www.hivtrainingny.org/PeerCert/ViolationComplaint",
                    foundational: "https://www.hivtrainingny.org/Home/FoundationalTraining",
                    orgAssessment: "https://www.hivtrainingny.org/PeerCert/OrgAssessment",
                    supervisorsFaq: "https://www.hivtrainingny.org/PeerCert/SupervisorFAQ",
                    integratingPeers: "https://www.hivtrainingny.org/PeerCert/PeerWorkersInHealthcare",
                    benefits: "https://www.hivtrainingny.org/PeerCert/BenefitsCounseling",
                    empireFaq: "https://www.hivtrainingny.org/PeerCert/EmpireStateCreditsFAQ",
                    listserv: "https://www.hivtrainingny.org/PeerCert/EmploymentListserv"
                }
            };
        },

        computed: {
            peerResources() {
                return [
                    {
                        title: "Frequently Asked Questions (FAQs)",
                        desc: "Eligibility, tracks, training requirements, practicum, exam details, and application guidance.",
                        href: this.links.faq
                    },
                    {
                        title: "Process, Requirements, Application",
                        desc: "Overview of the certification process and program requirements.",
                        href: this.links.process
                    },
                    {
                        title: "Course Tracker Forms",
                        desc: "Track required coursework for HIV, HCV, HR, PrEP, and Criminal Justice certification tracks.",
                        href: this.links.courseTrackers
                    },
                    {
                        title: "NYS Peer Worker Certification Program: Course Catalog",
                        desc: "Full list of training opportunities available for the certification program.",
                        href: this.links.catalog
                    },
                    {
                        title: "Peer Worker Self-Care Worksheet",
                        desc: "Self-reflection tool to support readiness and wellness planning.",
                        href: this.links.selfCare
                    },
                    {
                        title: "Foundational Training",
                        desc: "Recommended preparation before beginning certification training.",
                        href: this.links.foundational
                    },
                    {
                        title: "Instructions for Suicide Prevention Course",
                        desc: "Guidance for registering for and completing the Living Works START course.",
                        href: this.links.suicide
                    },
                    {
                        title: "Supervisor Evaluation Tool",
                        desc: "Required practicum evaluation resource for certification applicants and supervisors.",
                        href: this.links.supervisorEval
                    },
                    {
                        title: "Core Competencies",
                        desc: "Review the competencies certified peer workers are expected to demonstrate.",
                        href: this.links.competencies
                    },
                    {
                        title: "Exam Study Guides",
                        desc: "Study tools to help prepare for the online peer certification exam.",
                        href: this.links.studyGuides
                    }
                ];
            },

            certifiedResources() {
                return [
                    {
                        title: "Continuing Education Units (CEUs) Guidance",
                        desc: "Guidance for maintaining certification and annual continuing education expectations.",
                        href: this.links.ceus
                    },
                    {
                        title: "Code of Ethics",
                        desc: "Principles that guide certified peer workers in their roles, relationships, and responsibilities.",
                        href: this.links.ethics
                    },
                    {
                        title: "Code of Ethics Violation Complaint Process and Forms",
                        desc: "Procedures and complaint forms related to ethics concerns.",
                        href: this.links.ethicsViolation
                    }
                ];
            },

            organizationResources() {
                return [
                    {
                        title: "NYS Peer Worker Certification Program Supervisor Evaluation",
                        desc: "Required evaluation tool that can also be used throughout the practicum.",
                        href: this.links.supervisorEval
                    },
                    {
                        title: "Peer Certification Supervisors FAQ",
                        desc: "Guidance for supervisors supporting certification applicants.",
                        href: this.links.supervisorsFaq
                    },
                    {
                        title: "Integrating Peer Workers into Health Care Services",
                        desc: "Webinar resource for directors and supervisors.",
                        href: this.links.integratingPeers
                    },
                    {
                        title: "Organizational Assessment for Peer-Delivered Services",
                        desc: "Resource to help organizations assess readiness and capacity to provide peer services.",
                        href: this.links.orgAssessment
                    }
                ];
            },

            employmentResources() {
                return [
                    {
                        title: "Benefits Counseling Program",
                        desc: "Support for understanding how employment earnings may affect public benefits.",
                        href: this.links.benefits
                    },
                    {
                        title: "Empire State University Credits FAQ",
                        desc: "Learn about available academic credit-related guidance.",
                        href: this.links.empireFaq
                    },
                    {
                        title: "Peer Worker Employment Opportunities E-Mail LISTSERV",
                        desc: "Get job opportunities, program updates, and training announcements by email.",
                        href: this.links.listserv
                    }
                ];
            }
        },

        methods: {
            goToApply() {
                this.$router.push("/peer-certification/apply");
            },

            getUserGuid() {
                return localStorage.getItem("userId");
            },

            async apiFetch(url, options = {}) {
                const fullUrl = url.startsWith("/api")
                    ? url
                    : `/api${url.startsWith("/") ? "" : "/"}${url}`;

                const opts = {
                    ...options,
                    credentials: "include",
                    headers: {
                        ...(options.headers || {}),
                        Accept: "application/json"
                    }
                };

                return await fetch(fullUrl, opts);
            },

            unwrapDotNetList(data) {
                if (Array.isArray(data)) return data;
                if (data && Array.isArray(data.$values)) return data.$values;
                return [];
            },

            async openTrackModal() {
                this.showTrackModal = true;
                this.trackingLoading = true;
                this.trackingError = "";
                this.applicationTracks = [];

                const userId = this.getUserGuid();

                if (!userId) {
                    this.trackingLoading = false;
                    this.trackingError = "Please log in to track your peer certification applications.";
                    return;
                }

                try {
                    const res = await this.apiFetch(`/api/PeerCertification/track/${userId}`, {
                        method: "GET"
                    });

                    if (!res.ok) {
                        this.trackingError = await res.text();
                        return;
                    }

                    const data = await res.json();

                    // ✅ important fix
                    this.applicationTracks = this.unwrapDotNetList(data);

                    // optional debug
                    console.log("track api response:", data);
                    console.log("applicationTracks:", this.applicationTracks);
                } catch (e) {
                    this.trackingError = e?.message || "Unable to load your application tracking details.";
                } finally {
                    this.trackingLoading = false;
                }
            }, 

            closeTrackModal() {
                this.showTrackModal = false;
            },

            formatDate(value) {
                if (!value) return "—";
                const d = new Date(value);
                if (Number.isNaN(d.getTime())) return "—";
                return d.toLocaleDateString();
            },

            yesNo(value) {
                return value ? "Yes" : "No";
            },

            statusClass(status) {
                const s = (status || "").toLowerCase();

                if (s === "approved") return "approved";
                if (s === "disapproved") return "disapproved";
                return "pending";
            }
        }
    };</script>

<style scoped>
    .peer-page {
        width: min(1380px, calc(100% - 32px));
        margin: 24px auto 40px;
        color: #1f2937;
    }

    .hero {
        display: grid;
        grid-template-columns: 1.35fr 0.85fr;
        gap: 22px;
        padding: 32px;
        border-radius: 28px;
        background: radial-gradient(circle at top right, rgba(124, 58, 237, 0.12), transparent 28%), linear-gradient(180deg, #ffffff 0%, #faf8ff 100%);
        border: 1px solid #e9e4f2;
        box-shadow: 0 18px 40px rgba(15, 23, 42, 0.07);
    }

    .hero-badge,
    .cta-badge {
        display: inline-flex;
        align-items: center;
        width: fit-content;
        padding: 8px 14px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.08);
        color: #43285d;
        font-size: 12px;
        font-weight: 800;
        letter-spacing: 0.04em;
        text-transform: uppercase;
        margin-bottom: 14px;
    }

    .hero h1 {
        margin: 0 0 14px;
        font-size: 40px;
        line-height: 1.12;
        font-weight: 900;
        color: #1e1630;
        max-width: 900px;
    }

    .hero-text {
        margin: 0;
        font-size: 16px;
        line-height: 1.8;
        color: #4b5563;
        max-width: 860px;
    }

    .hero-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 14px;
        margin-top: 24px;
    }

    .hero-side {
        display: grid;
        gap: 16px;
        align-content: start;
    }

    .stat-card {
        padding: 22px;
        border-radius: 22px;
        background: #ffffff;
        border: 1px solid #ebe7f2;
        box-shadow: 0 12px 28px rgba(15, 23, 42, 0.05);
    }

    .stat-number {
        font-size: 34px;
        font-weight: 900;
        color: #43285d;
        line-height: 1;
        margin-bottom: 8px;
    }

    .stat-label {
        font-size: 14px;
        line-height: 1.6;
        color: #6b7280;
    }

    .section {
        margin-top: 28px;
        padding: 28px;
        border-radius: 24px;
        background: #ffffff;
        border: 1px solid #e5e7eb;
        box-shadow: 0 14px 32px rgba(15, 23, 42, 0.05);
    }

    .section-head {
        margin-bottom: 18px;
    }

        .section-head h2 {
            margin: 0 0 8px;
            font-size: 28px;
            font-weight: 900;
            color: #1f1630;
        }

        .section-head p {
            margin: 0;
            font-size: 15px;
            line-height: 1.75;
            color: #6b7280;
        }

    .info-grid,
    .resource-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 18px;
    }

    .info-card,
    .resource-card,
    .panel {
        border: 1px solid #ebeef2;
        border-radius: 22px;
        background: #fff;
        padding: 22px;
    }

        .info-card h3,
        .resource-card h3,
        .panel h3,
        .support-network h3,
        .cta-card h2 {
            margin: 0 0 10px;
            font-size: 19px;
            font-weight: 850;
            color: #1f1630;
        }

        .info-card p,
        .resource-card p,
        .panel p,
        .cta-card p,
        .eligibility-box p,
        .small-note {
            margin: 0;
            font-size: 14px;
            line-height: 1.75;
            color: #4b5563;
        }

            .info-card a,
            .resource-card a,
            .simple-link-card,
            .small-note a,
            .cta-card a {
                text-decoration: none;
            }

            .info-card a,
            .resource-card a,
            .small-note a,
            .cta-card a {
                color: #43285d;
                font-weight: 800;
            }

    .clean-list {
        margin: 0;
        padding-left: 18px;
        display: grid;
        gap: 10px;
        color: #374151;
        font-size: 14px;
        line-height: 1.65;
    }

    .link-stack {
        display: grid;
        gap: 12px;
    }

        .link-stack a {
            color: #43285d;
            font-weight: 800;
            text-decoration: none;
        }

    .update-banner {
        padding: 24px;
        border-radius: 22px;
        background: linear-gradient(180deg, #f7fafc 0%, #f8f5ff 100%);
        border: 1px solid #e7e2f1;
        margin-bottom: 18px;
    }

    .update-chip {
        display: inline-flex;
        padding: 7px 12px;
        border-radius: 999px;
        background: rgba(6, 95, 70, 0.10);
        color: #065f46;
        font-size: 12px;
        font-weight: 900;
        margin-bottom: 12px;
    }

    .update-banner h3 {
        margin: 0 0 10px;
        font-size: 22px;
        font-weight: 900;
        color: #1f1630;
    }

    .update-banner p {
        margin: 0;
        font-size: 15px;
        line-height: 1.8;
        color: #4b5563;
    }

    .two-col-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 18px;
    }

    .support-network {
        margin-top: 18px;
        padding: 22px;
        border-radius: 22px;
        border: 1px solid #e8edf1;
        background: #fbfcfe;
    }

    .meeting-list {
        display: flex;
        flex-wrap: wrap;
        gap: 12px;
        margin-top: 16px;
    }

    .meeting-item {
        padding: 10px 14px;
        border-radius: 999px;
        background: #ffffff;
        border: 1px solid #e5e7eb;
        font-size: 13px;
        font-weight: 800;
        color: #374151;
    }

    .resource-card {
        display: flex;
        gap: 14px;
    }

    .resource-icon {
        width: 42px;
        height: 42px;
        min-width: 42px;
        border-radius: 14px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: rgba(67, 40, 93, 0.08);
        color: #43285d;
        font-weight: 900;
    }

    .resource-body {
        display: flex;
        flex-direction: column;
        gap: 8px;
    }

    .card-list {
        display: grid;
        gap: 14px;
    }

    .simple-link-card {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 18px;
        padding: 20px 22px;
        border-radius: 20px;
        border: 1px solid #e7ebf0;
        background: #fff;
        transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease;
    }

        .simple-link-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 14px 28px rgba(15, 23, 42, 0.06);
            border-color: #d9dce3;
        }

        .simple-link-card h3 {
            margin: 0 0 6px;
            font-size: 18px;
            font-weight: 850;
            color: #1f1630;
        }

        .simple-link-card p {
            margin: 0;
            font-size: 14px;
            line-height: 1.7;
            color: #6b7280;
        }

        .simple-link-card span {
            flex: 0 0 auto;
            color: #43285d;
            font-weight: 900;
            font-size: 13px;
        }

    .eligibility-box {
        padding: 22px;
        border-radius: 22px;
        background: linear-gradient(180deg, #ffffff 0%, #f9fbff 100%);
        border: 1px solid #e4e9f0;
    }

    .tracks-grid {
        display: flex;
        flex-wrap: wrap;
        gap: 12px;
        margin-top: 18px;
    }

    .track-pill {
        padding: 10px 16px;
        border-radius: 999px;
        background: #f8fafc;
        border: 1px solid #e5e7eb;
        font-size: 13px;
        font-weight: 900;
        color: #374151;
    }

    .cta-section {
        margin-top: 28px;
    }

    .cta-card {
        display: grid;
        grid-template-columns: 1.2fr auto;
        gap: 22px;
        align-items: center;
        padding: 30px;
        border-radius: 26px;
        background: linear-gradient(180deg, #201132 0%, #43285d 100%);
        color: #ffffff;
        box-shadow: 0 20px 40px rgba(31, 22, 48, 0.25);
    }

        .cta-card h2 {
            color: #ffffff;
            font-size: 30px;
        }

        .cta-card p,
        .cta-card .small-note,
        .cta-card .small-note a {
            color: rgba(255, 255, 255, 0.88);
        }

    .cta-actions {
        display: grid;
        gap: 12px;
        min-width: 240px;
    }

    .btn {
        border: none;
        border-radius: 999px;
        padding: 12px 20px;
        min-height: 46px;
        font-size: 14px;
        font-weight: 850;
        cursor: pointer;
        text-decoration: none;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        transition: transform 0.16s ease, box-shadow 0.16s ease, background 0.16s ease;
    }

        .btn:hover {
            transform: translateY(-1px);
        }

    .btn-primary {
        background: #43285d;
        color: #fff;
        box-shadow: 0 12px 24px rgba(67, 40, 93, 0.24);
    }

    .btn-secondary {
        background: #fff;
        color: #111827;
        border: 1px solid #e5e7eb;
    }

    .large-btn {
        width: 100%;
    }

    @media (max-width: 1100px) {
        .hero,
        .cta-card,
        .info-grid,
        .resource-grid,
        .two-col-grid {
            grid-template-columns: 1fr;
        }

        .cta-actions {
            min-width: 0;
        }
    }

    @media (max-width: 760px) {
        .peer-page {
            width: min(100% - 20px, 100%);
            margin: 14px auto 28px;
        }

        .hero,
        .section,
        .cta-card {
            padding: 20px 16px;
            border-radius: 20px;
        }

            .hero h1 {
                font-size: 30px;
            }

        .section-head h2 {
            font-size: 24px;
        }

        .cta-card h2 {
            font-size: 24px;
        }

        .simple-link-card {
            flex-direction: column;
            align-items: flex-start;
        }

        .hero-actions {
            flex-direction: column;
        }

            .hero-actions .btn {
                width: 100%;
            }
    }
    .btn-outline {
        background: transparent;
        color: #43285d;
        border: 1px solid rgba(67, 40, 93, 0.22);
        box-shadow: inset 0 0 0 1px rgba(67, 40, 93, 0.02);
    }

        .btn-outline:hover {
            background: rgba(67, 40, 93, 0.06);
        }

    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(15, 23, 42, 0.58);
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 18px;
        z-index: 9999;
        backdrop-filter: blur(4px);
    }

    .track-modal {
        width: min(980px, 100%);
        background: #ffffff;
        border-radius: 24px;
        border: 1px solid #e5e7eb;
        box-shadow: 0 28px 70px rgba(15, 23, 42, 0.22);
        overflow: hidden;
        display: flex;
        flex-direction: column;
        max-height: 90vh;
    }

    .track-modal-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 18px;
        padding: 24px 24px 18px;
        background: linear-gradient(180deg, #faf8ff 0%, #ffffff 100%);
        border-bottom: 1px solid #ece8f3;
    }

        .track-modal-header h3 {
            margin: 0 0 6px;
            font-size: 24px;
            font-weight: 900;
            color: #1f1630;
        }

        .track-modal-header p {
            margin: 0;
            font-size: 14px;
            line-height: 1.7;
            color: #6b7280;
        }

    .track-modal-badge {
        display: inline-flex;
        width: fit-content;
        padding: 7px 12px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.08);
        color: #43285d;
        font-size: 11px;
        font-weight: 900;
        text-transform: uppercase;
        letter-spacing: 0.05em;
        margin-bottom: 10px;
    }

    .modal-close-btn {
        border: none;
        background: #f4f4f5;
        color: #111827;
        width: 40px;
        height: 40px;
        border-radius: 999px;
        cursor: pointer;
        font-size: 16px;
        font-weight: 900;
        flex: 0 0 auto;
    }

        .modal-close-btn:hover {
            background: #e5e7eb;
        }

    .track-modal-body {
        padding: 22px 24px;
        overflow: auto;
    }

    .track-state-card {
        padding: 18px 20px;
        border-radius: 18px;
        background: #f8fafc;
        border: 1px solid #e5e7eb;
        color: #374151;
        font-size: 14px;
        font-weight: 700;
    }

    .track-error-card {
        background: #fff7f7;
        border-color: #fecaca;
        color: #991b1b;
    }

    .track-summary-row {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 14px;
        margin-bottom: 18px;
    }

    .track-summary-card {
        padding: 18px;
        border-radius: 18px;
        border: 1px solid #e9edf2;
        background: linear-gradient(180deg, #ffffff 0%, #fafcff 100%);
    }

        .track-summary-card span {
            display: block;
            font-size: 12px;
            color: #6b7280;
            margin-bottom: 8px;
            font-weight: 800;
            text-transform: uppercase;
            letter-spacing: 0.04em;
        }

        .track-summary-card strong {
            font-size: 18px;
            font-weight: 900;
            color: #111827;
        }

    .track-table-wrap {
        display: flex;
        flex-direction: column;
        gap: 14px;
    }

    .track-table {
        width: 100%;
        border-collapse: collapse;
        overflow: hidden;
        border-radius: 18px;
        border: 1px solid #e5e7eb;
        background: #fff;
    }

        .track-table th,
        .track-table td {
            padding: 15px 18px;
            text-align: left;
            border-bottom: 1px solid #eef2f7;
            vertical-align: middle;
        }

        .track-table th {
            width: 280px;
            background: #fafafa;
            color: #374151;
            font-size: 13px;
            font-weight: 900;
        }

        .track-table td {
            color: #111827;
            font-size: 14px;
            font-weight: 600;
        }

        .track-table tr:last-child th,
        .track-table tr:last-child td {
            border-bottom: none;
        }

    .status-pill {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 108px;
        padding: 8px 12px;
        border-radius: 999px;
        font-size: 12px;
        font-weight: 900;
        text-transform: uppercase;
        letter-spacing: 0.04em;
    }

    .approved {
        color: #065f46 !important;
        background: rgba(6, 95, 70, 0.12);
    }

    .pending {
        color: #92400e !important;
        background: rgba(245, 158, 11, 0.16);
    }

    .disapproved {
        color: #991b1b !important;
        background: rgba(239, 68, 68, 0.14);
    }

    .subtle-inline {
        color: #6b7280;
        font-size: 13px;
        margin-left: 6px;
        font-weight: 600;
    }

    .track-help-note {
        padding: 16px 18px;
        border-radius: 16px;
        background: #f8fafc;
        border: 1px solid #e5e7eb;
        color: #4b5563;
        font-size: 13px;
        line-height: 1.7;
    }

    .track-modal-footer {
        display: flex;
        justify-content: flex-end;
        padding: 18px 24px 22px;
        border-top: 1px solid #eceff3;
        background: #ffffff;
    }

    @media (max-width: 900px) {
        .track-summary-row {
            grid-template-columns: 1fr;
        }

        .track-table th,
        .track-table td {
            padding: 13px 14px;
        }

        .track-table th {
            width: 44%;
        }
    }

    @media (max-width: 640px) {
        .track-modal {
            border-radius: 18px;
        }

        .track-modal-header,
        .track-modal-body,
        .track-modal-footer {
            padding-left: 16px;
            padding-right: 16px;
        }

        .track-table,
        .track-table tbody,
        .track-table tr,
        .track-table th,
        .track-table td {
            display: block;
            width: 100%;
        }

            .track-table tr {
                border-bottom: 1px solid #eef2f7;
                padding: 10px 0;
            }

            .track-table th {
                background: transparent;
                border-bottom: none;
                padding-bottom: 6px;
            }

            .track-table td {
                border-bottom: none;
                padding-top: 0;
            }
    }
    .track-table-scroll {
        width: 100%;
        overflow-x: auto;
        border: 1px solid #e5e7eb;
        border-radius: 18px;
        background: #fff;
    }

    .multi-track-table {
        min-width: 1250px;
        border: none;
        border-radius: 0;
    }

        .multi-track-table thead th {
            position: sticky;
            top: 0;
            background: #f8fafc;
            z-index: 1;
            white-space: nowrap;
        }

        .multi-track-table th,
        .multi-track-table td {
            white-space: nowrap;
        }

        .multi-track-table tbody tr:nth-child(even) {
            background: #fcfcfd;
        }

        .multi-track-table tbody tr:hover {
            background: #f7f4fb;
        }

    .cell-good {
        color: #065f46;
        font-weight: 800;
    }

    .cell-muted {
        color: #6b7280;
        font-weight: 700;
    }
</style>