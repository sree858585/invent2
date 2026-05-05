<template>
    <div class="help-assistant">
        <button class="help-launcher" @click="toggleAssistant">
            <span class="bot-avatar">💬</span>
            <span class="bot-text">
                <strong>Training Help</strong>
                <small>Hi! Need help?</small>
            </span>
        </button>

        <div v-if="isOpen" class="help-window">
            <div class="help-window-header">
                <div>
                    <strong>Hi, I’m Training Help</strong>
                    <small>Search help topics or choose below</small>
                </div>
                <button @click="isOpen = false">×</button>
            </div>

            <input v-model="search"
                   class="help-search"
                   placeholder="Search: certificate, launch, register..." />

            <div class="category-tabs">
                <button v-for="cat in categories"
                        :key="cat"
                        :class="{ active: selectedCategory === cat }"
                        @click="selectedCategory = cat">
                    {{ cat }}
                </button>
            </div>

            <div class="faq-list">
                <button v-for="item in filteredFaqs"
                        :key="item.question"
                        class="faq-btn"
                        @click="selectedFaq = item">
                    <span>{{ item.icon }}</span>
                    {{ item.question }}
                </button>
            </div>

            <div v-if="filteredFaqs.length === 0" class="empty-help">
                No help topic found. Try searching another keyword.
            </div>

            <div v-if="selectedFaq" class="faq-answer">
                <strong>{{ selectedFaq.question }}</strong>
                <p>{{ selectedFaq.answer }}</p>

                <button v-if="selectedFaq.route"
                        class="go-btn"
                        @click="goTo(selectedFaq.route)">
                    Go there →
                </button>
            </div>
        </div>
    </div>
</template>

<script>export default {
        name: "HelpAssistant",

        data() {
            return {
                isOpen: false,
                search: "",
                selectedCategory: "Popular",
                selectedFaq: null,

                faqs: [
                    {
                        category: "Popular",
                        icon: "📚",
                        question: "How do I register for a course?",
                        answer: "Go to Upcoming Courses, open a course, review the details, and click Register.",
                        route: "/course-list-page",
                        keywords: "register course upcoming enroll"
                    },
                    {
                        category: "Popular",
                        icon: "▶️",
                        question: "How do I launch an online training?",
                        answer: "Go to My Dashboard > My Learnings. In the In Progress tab, click Launch Course.",
                        route: "/my-courses/registered",
                        keywords: "launch online scorm start training"
                    },
                    {
                        category: "Popular",
                        icon: "📜",
                        question: "Where can I view my certificate?",
                        answer: "You can view certificates from My Certificates or from the Attended tab in My Learnings.",
                        route: "/my-certificates",
                        keywords: "certificate completed attended download print"
                    },
                    {
                        category: "Popular",
                        icon: "❌",
                        question: "How do I drop a course?",
                        answer: "Go to My Learnings, find the course, click Drop, and confirm the action.",
                        route: "/my-courses/registered",
                        keywords: "drop cancel remove registration"
                    },

                    {
                        category: "Courses",
                        icon: "🔎",
                        question: "How do I search or filter courses?",
                        answer: "Use the Courses section or Upcoming Courses page. You can filter by format such as Online, In Person, Webinar, Hybrid, or New.",
                        route: "/course-list/all",
                        keywords: "filter search format online in person webinar hybrid"
                    },
                    {
                        category: "Courses",
                        icon: "⏳",
                        question: "Why am I on the waitlist?",
                        answer: "You are waitlisted when a course has no available seats. If a seat opens, admins can move users from the waitlist.",
                        route: "/my-courses/registered",
                        keywords: "waitlist seats max seats full"
                    },
                    {
                        category: "Courses",
                        icon: "📅",
                        question: "Where can I see training dates?",
                        answer: "Open the course details or visit the Training Calendar page to see scheduled trainings.",
                        route: "/training-calendar",
                        keywords: "calendar date schedule training"
                    },
                    {
                        category: "Courses",
                        icon: "🖥️",
                        question: "What is an online training?",
                        answer: "Online trainings are self-paced SCORM courses. You can launch them from My Learnings and your progress can be saved.",
                        route: "/my-courses/registered",
                        keywords: "online training scorm self paced progress"
                    },

                    {
                        category: "My Learning",
                        icon: "📊",
                        question: "Where can I track my progress?",
                        answer: "Go to My Learnings. Online trainings show progress percentage beside the course.",
                        route: "/my-courses/registered",
                        keywords: "progress percentage my learnings"
                    },
                    {
                        category: "My Learning",
                        icon: "🔁",
                        question: "Can I resume an online course?",
                        answer: "Yes. If progress is saved, the button changes to Resume Course. Click it to continue.",
                        route: "/my-courses/registered",
                        keywords: "resume continue progress online course"
                    },
                    {
                        category: "My Learning",
                        icon: "✅",
                        question: "When does a course move to Attended?",
                        answer: "For online training, it moves to Attended after successful completion. For other courses, attendance is marked by an admin.",
                        route: "/my-courses/registered",
                        keywords: "attended completed status"
                    },
                    {
                        category: "My Learning",
                        icon: "📌",
                        question: "Where can I see cancelled or dropped courses?",
                        answer: "Go to My Learnings and select the Cancelled or Dropped tab.",
                        route: "/my-courses/registered",
                        keywords: "cancelled dropped absent status"
                    },

                    {
                        category: "Profile",
                        icon: "👤",
                        question: "Where can I update my profile?",
                        answer: "Open My Dashboard and click View Profile. You can review your personal information there.",
                        route: this.getProfileRoute(),
                        keywords: "profile user information account"
                    },
                    {
                        category: "Profile",
                        icon: "♿",
                        question: "How do I request ADA accommodation?",
                        answer: "During course registration, select the ADA option and enter details. The information is saved with your registration.",
                        route: "/course-list-page",
                        keywords: "ada accommodation disability request"
                    },

                    {
                        category: "Admin",
                        icon: "🛠️",
                        question: "How do admins manage courses?",
                        answer: "Admins and managers can use Course Management to edit, cancel, email users, add users, drop users, and manage attendance.",
                        route: "/course-management",
                        keywords: "admin manage course cancel email attendance"
                    },
                    {
                        category: "Admin",
                        icon: "🏷️",
                        question: "How do I create a training title?",
                        answer: "Go to System Management > Training Titles. You can create title metadata, upload images, and upload SCORM ZIP packages.",
                        route: "/system/training-title",
                        keywords: "training title create scorm upload zip"
                    },
                    {
                        category: "Admin",
                        icon: "🧑‍🏫",
                        question: "How do I manage instructors?",
                        answer: "Go to System Management > Instructor Management to add, edit, archive, or activate instructors.",
                        route: "/system/instructor-management",
                        keywords: "instructor management add edit archive"
                    },
                    {
                        category: "Admin",
                        icon: "🏢",
                        question: "How do I manage training centers?",
                        answer: "Go to System Management > Training Centers to add, edit, activate, or deactivate centers.",
                        route: "/system/training-center",
                        keywords: "training center site management"
                    }
                ]
            };
        },

        computed: {
            categories() {
                const role = localStorage.getItem("userRole");
                const base = ["Popular", "Courses", "My Learning", "Profile"];
                if (role === "Admin" || role === "Manager") base.push("Admin");
                return base;
            },

            filteredFaqs() {
                const q = this.search.toLowerCase().trim();

                return this.faqs.filter(item => {
                    const role = localStorage.getItem("userRole");

                    if (item.category === "Admin" && role !== "Admin" && role !== "Manager") {
                        return false;
                    }

                    const categoryMatch = item.category === this.selectedCategory;

                    if (!q) return categoryMatch;

                    const searchMatch =
                        item.question.toLowerCase().includes(q) ||
                        item.answer.toLowerCase().includes(q) ||
                        item.keywords.toLowerCase().includes(q);

                    return searchMatch;
                });
            }
        },

        methods: {
            toggleAssistant() {
                this.isOpen = !this.isOpen;
                if (this.isOpen && !this.selectedFaq) {
                    this.selectedFaq = null;
                }
            },

            goTo(route) {
                if (!route) return;
                this.isOpen = false;
                this.$router.push(route);
            },

            getProfileRoute() {
                const userId = localStorage.getItem("userId");
                return userId ? `/profile/view/${userId}` : "/home";
            }
        }
    };</script>

<style scoped>
    .help-assistant {
        position: fixed;
        right: 24px;
        bottom: 24px;
        z-index: 99999;
        width: 300px;
    }

    .help-launcher {
        width: 100%;
        border: 1px solid #d7def0;
        border-radius: 18px;
        padding: 12px;
        background: #ffffff;
        color: #43285d;
        display: flex;
        align-items: center;
        gap: 10px;
        cursor: pointer;
        box-shadow: 0 14px 34px rgba(15, 23, 42, 0.2);
    }

    .bot-avatar {
        width: 38px;
        height: 38px;
        border-radius: 50%;
        background: #f1eafa;
        display: grid;
        place-items: center;
    }

    .bot-text {
        text-align: left;
    }

        .bot-text small {
            display: block;
            font-size: 11px;
            color: #6b7280;
        }

    .help-window {
        margin-bottom: 12px;
        background: white;
        color: #111827;
        border-radius: 20px;
        padding: 14px;
        box-shadow: 0 24px 60px rgba(15, 23, 42, 0.28);
        border: 1px solid #e5e7eb;
        max-height: 560px;
        overflow-y: auto;
    }

    .help-window-header {
        display: flex;
        justify-content: space-between;
        gap: 10px;
        margin-bottom: 12px;
    }

        .help-window-header small {
            display: block;
            color: #6b7280;
            font-size: 12px;
        }

        .help-window-header button {
            border: none;
            background: #f3f4f6;
            border-radius: 50%;
            width: 28px;
            height: 28px;
            cursor: pointer;
        }

    .help-search {
        width: 100%;
        border: 1px solid #e5e7eb;
        border-radius: 12px;
        padding: 10px;
        margin-bottom: 10px;
    }

    .category-tabs {
        display: flex;
        gap: 6px;
        overflow-x: auto;
        margin-bottom: 10px;
    }

        .category-tabs button {
            border: none;
            border-radius: 999px;
            padding: 7px 11px;
            background: #f3f4f6;
            color: #374151;
            font-size: 12px;
            font-weight: 700;
            cursor: pointer;
            white-space: nowrap;
        }

            .category-tabs button.active {
                background: #43285d;
                color: white;
            }

    .faq-list {
        display: flex;
        flex-direction: column;
        gap: 8px;
    }

    .faq-btn {
        width: 100%;
        text-align: left;
        border: none;
        background: #f6f3fb;
        color: #43285d;
        border-radius: 14px;
        padding: 10px;
        font-weight: 700;
        cursor: pointer;
        line-height: 1.35;
    }

        .faq-btn:hover {
            background: #ebe2f6;
        }

    .faq-answer {
        margin-top: 12px;
        background: #f9fafb;
        border-left: 4px solid #6e528d;
        border-radius: 14px;
        padding: 12px;
        font-size: 13px;
    }

        .faq-answer p {
            margin: 6px 0 0;
            line-height: 1.5;
            color: #374151;
        }

    .go-btn {
        margin-top: 10px;
        border: none;
        border-radius: 999px;
        background: #43285d;
        color: white;
        padding: 8px 14px;
        font-weight: 700;
        cursor: pointer;
    }

    .empty-help {
        color: #6b7280;
        background: #f9fafb;
        padding: 12px;
        border-radius: 12px;
        font-size: 13px;
    }

    @media (max-width: 600px) {
        .help-assistant {
            right: 14px;
            left: 14px;
            bottom: 14px;
            width: auto;
        }
    }
</style>