<template>
    <div class="reports-page">
        <h2>Reports</h2>

        <!--<section class="date-card">
        <div class="filter-field">
            <label>From Date</label>
            <input type="date" v-model="fromDate" />
        </div>

        <div class="filter-field">
            <label>To Date</label>
            <input type="date" v-model="toDate" />
        </div>

        <button class="reset-btn" @click="resetAll">Reset</button>
    </section>-->

        <section class="course-select-card">
            <div class="course-list-header">
                <div>
                    <h3>Course Selection</h3>
                    <p>{{ filteredCourses.length }} courses found</p>
                </div>

                <label class="select-all">
                    <input type="checkbox"
                           :checked="isAllSelected"
                           @change="toggleSelectAll" />
                    Select All
                </label>
            </div>

            <div class="table-search">
                <input type="text"
                       v-model="searchQuery"
                       placeholder="Search courses by title, site, location, format..." />
            </div>

            <div class="course-table-wrapper">
                <table>
                    <thead>
                        <tr>
                            <th></th>
                            <th>Course Title</th>
                            <th>Date</th>
                            <th>Site</th>
                            <th>Format</th>
                            <th>Location</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="course in filteredCourses" :key="course.courseSysId">
                            <td>
                                <input type="checkbox"
                                       :value="course.courseSysId"
                                       v-model="selectedCourseIds" />
                            </td>
                            <td>{{ course.subjectTitle }}</td>
                            <td>{{ formatDate(course.courseDate) }}</td>
                            <td>{{ course.siteName || 'N/A' }}</td>
                            <td>{{ course.formatLabel || 'N/A' }}</td>
                            <td>{{ course.trainingLocation || course.city || 'N/A' }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </section>
        <section class="report-options-card">
            <div class="report-options-header">
                <div>
                    <h3>Report Options</h3>
                    <p>Select date range, choose one report type, then download.</p>
                </div>

                <div class="report-header-actions">
                    <div class="filter-field small-date">
                        <label>From Date</label>
                        <input type="date" v-model="fromDate" />
                    </div>

                    <div class="filter-field small-date">
                        <label>To Date</label>
                        <input type="date" v-model="toDate" />
                    </div>

                    <button class="reset-btn" @click="resetAll">Reset</button>

                    <div v-if="selectedReport" class="download-actions">

                        <button class="download-btn chart-btn" @click.stop="viewChart">

                            View Chart

                        </button>

                        <button class="download-btn pdf-btn" @click.stop="downloadReport('pdf')">

                            Download PDF

                        </button>

                        <button class="download-btn csv-btn" @click.stop="downloadReport('csv')">

                            Download CSV

                        </button>

                    </div>
                </div>
            </div>

            <div class="report-table-wrapper">
                <table>
                    <thead>
                        <tr>
                            <th></th>
                            <th>Report Name</th>
                            <th>Description</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="report in reportOptions"
                            :key="report.key"
                            :class="{ selected: selectedReport === report.key }"
                            @click="selectedReport = report.key">
                            <td>
                                <input type="radio"
                                       name="reportOption"
                                       :value="report.key"
                                       v-model="selectedReport" />
                            </td>
                            <td>{{ report.name }}</td>
                            <td>{{ report.description }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </section>

        <div v-if="showChartModal" class="chart-modal-overlay" @click.self="showChartModal = false">
            <div class="chart-modal">
                <div class="chart-modal-header">
                    <div>
                        <h3>{{ chartTitle }}</h3>
                        <p>{{ fromDate }} to {{ toDate }}</p>
                    </div>

                    <button class="chart-close" @click="showChartModal = false">&times;</button>
                </div>

                <apexchart width="100%"
                           height="380"
                           type="bar"
                           :options="chartOptions"
                           :series="chartSeries">
                </apexchart>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";

    export default {
        name: "ReportsPage",

        data() {
            return {
                fromDate: "",
                toDate: "",
                searchQuery: "",
                courses: [],
                selectedCourseIds: [],
                loading: false,

                selectedReport: "",
                showChartModal: false,
                chartTitle: "",
                chartSeries: [],
                chartOptions: {
                    chart: {
                        toolbar: { show: true }
                    },
                    plotOptions: {
                        bar: {
                            borderRadius: 6,
                            columnWidth: "45%"
                        }
                    },
                    dataLabels: {
                        enabled: false
                    },
                    xaxis: {
                        categories: []
                    },
                    colors: ["#43285D"]
                },

                reportOptions: [
                    { key: "trainingsScheduled", name: "Trainings Scheduled", description: "Courses scheduled within the selected date range." },
                    { key: "attendanceParticipation", name: "Attendance & Participation", description: "Registration, attendance, and participation summary." },
                    { key: "deliveredScheduledRatio", name: "Delivered-to-Scheduled Ratio", description: "Compare delivered trainings against scheduled trainings." },
                    { key: "dayOfWeekDelivered", name: "Breakdown by Day of the Week - Delivered Only", description: "Delivered trainings grouped by weekday." },
                    { key: "deliveryFrequency", name: "Training Delivery by Frequency", description: "Training delivery frequency summary." },
                    { key: "popularTrainingTop3", name: "Most Popular Training by Total Attendance (Top 3)", description: "Top attended trainings." },
                    { key: "averageAttendance", name: "Top Trainings by Average Attendance", description: "Average attendance per delivery." },
                    { key: "cancelledByDay", name: "Cancelled Trainings by Day of the Week", description: "Cancelled trainings grouped by weekday." },
                    { key: "cancelledTrainings", name: "Cancelled Trainings", description: "List of cancelled trainings." },
                    { key: "trainingByMonth", name: "Training by Month", description: "Monthly training summary." },
                    { key: "trainerEngagement", name: "Trainer Engagement", description: "Instructor/trainer involvement summary." },
                    { key: "trainingType", name: "Type of Training", description: "Breakdown by format such as Face-to-Face, Webinar, Online, etc." },
                    { key: "repeatParticipants", name: "Repeat Participants", description: "Users who attended multiple trainings." },
                ],

            };
        },

        computed: {
            filteredCourses() {
                const q = this.searchQuery.toLowerCase().trim();

                if (!q) return this.courses;

                return this.courses.filter(c =>
                    (c.subjectTitle || "").toLowerCase().includes(q) ||
                    (c.siteName || "").toLowerCase().includes(q) ||
                    (c.formatLabel || "").toLowerCase().includes(q) ||
                    (c.trainingLocation || "").toLowerCase().includes(q) ||
                    (c.city || "").toLowerCase().includes(q)
                );
            },

            isAllSelected() {
                const list = Array.isArray(this.filteredCourses)
                    ? this.filteredCourses
                    : [];

                return (
                    list.length > 0 &&
                    list.every(c => this.selectedCourseIds.includes(c.courseSysId))
                );
            },
        },

        mounted() {
            this.loadCourses();
        },

        methods: {


            async loadCourses() {
                this.loading = true;

                try {
                    const res = await apiClient.get("/Reports/courses", {
                        headers: {
                            Accept: "application/json"
                        },
                        params: {
                            _: Date.now()
                        }
                    });

                    console.log("Courses response:", res.data);

                    const data = res.data?.$values ?? res.data;

                    this.courses = Array.isArray(data) ? data : [];
                } catch (err) {
                    console.error("Failed to load report courses:", err);
                    this.courses = [];
                } finally {
                    this.loading = false;
                }
            },

            validateReportInputs(requireDates = true, requireCourses = false) {
                if (!this.selectedReport) {
                    alert("Please select a report.");
                    return false;
                }

                if (requireDates && (!this.fromDate || !this.toDate)) {
                    alert("Please select both From Date and To Date.");
                    return false;
                }

                if (requireDates && new Date(this.fromDate) > new Date(this.toDate)) {
                    alert("From Date should be before To Date.");
                    return false;
                }

                if (requireCourses && this.selectedCourseIds.length === 0) {
                    alert("Please select at least one course.");
                    return false;
                }

                return true;
            },

            getReportDownloadConfig(type) {
                if (this.selectedReport === "trainingsScheduled") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/trainings-scheduled/pdf"
                            : "/Reports/trainings-scheduled/csv",
                        fileName: type === "pdf"
                            ? "Trainings_Scheduled_Report.pdf"
                            : "Trainings_Scheduled_Report.csv",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "deliveredScheduledRatio") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/delivered-scheduled-ratio/pdf"
                            : "/Reports/delivered-scheduled-ratio/csv",
                        fileName: type === "pdf"
                            ? "Delivered_Scheduled_Ratio_Report.pdf"
                            : "Delivered_Scheduled_Ratio_Report.csv",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "attendanceParticipation") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/attendance-participation/pdf"
                            : "/Reports/attendance-participation/csv",
                        fileName: type === "pdf"
                            ? "Attendance_Participation_Report.pdf"
                            : "Attendance_Participation_Report.csv",
                        params: {
                            courseIds: this.selectedCourseIds.join(",")
                        }
                    };
                }

                if (this.selectedReport === "dayOfWeekDelivered") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/day-of-week-delivered/pdf"
                            : "/Reports/day-of-week-delivered/csv",
                        fileName: type === "pdf"
                            ? "Delivered_By_Day_Of_Week_Report.pdf"
                            : "Delivered_By_Day_Of_Week_Report.csv",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "deliveryFrequency") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/training-delivery-frequency/pdf"
                            : "/Reports/training-delivery-frequency/csv",
                        fileName: type === "pdf"
                            ? "Training_Delivery_Frequency_Report.pdf"
                            : "Training_Delivery_Frequency_Report.csv",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "popularTrainingTop3") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/popular-training-top3/pdf"
                            : "/Reports/popular-training-top3/csv",
                        fileName: type === "pdf"
                            ? "Popular_Training_Top3_Report.pdf"
                            : "Popular_Training_Top3_Report.csv",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "averageAttendance") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/average-attendance/pdf"
                            : "/Reports/average-attendance/csv",
                        fileName: type === "pdf"
                            ? "Average_Attendance_Report.pdf"
                            : "Average_Attendance_Report.csv",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "cancelledByDay") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/cancelled-by-day/pdf"
                            : "/Reports/cancelled-by-day/csv",
                        fileName: type === "pdf"
                            ? "Cancelled_By_Day_Report.pdf"
                            : "Cancelled_By_Day_Report.csv",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "cancelledTrainings") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/cancelled-trainings/pdf"
                            : "/Reports/cancelled-trainings/csv",

                        fileName: type === "pdf"
                            ? "Cancelled_Trainings_Report.pdf"
                            : "Cancelled_Trainings_Report.csv",

                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "trainingByMonth") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/training-by-month/pdf"
                            : "/Reports/training-by-month/csv",

                        fileName: type === "pdf"
                            ? "Training_By_Month_Report.pdf"
                            : "Training_By_Month_Report.csv",

                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "trainerEngagement") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/trainer-engagement/pdf"
                            : "/Reports/trainer-engagement/csv",

                        fileName: type === "pdf"
                            ? "Trainer_Engagement_Report.pdf"
                            : "Trainer_Engagement_Report.csv",

                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }
                if (this.selectedReport === "trainingType") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/training-type/pdf"
                            : "/Reports/training-type/csv",

                        fileName: type === "pdf"
                            ? "Training_Type_Report.pdf"
                            : "Training_Type_Report.csv",

                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "repeatParticipants") {
                    return {
                        url: type === "pdf"
                            ? "/Reports/repeat-participants/pdf"
                            : "/Reports/repeat-participants/csv",

                        fileName: type === "pdf"
                            ? "Repeat_Participants_Report.pdf"
                            : "Repeat_Participants_Report.csv",

                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                return null;
            },

            getReportChartConfig() {
                if (this.selectedReport === "trainingsScheduled") {
                    return {
                        url: "/Reports/trainings-scheduled/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "deliveredScheduledRatio") {
                    return {
                        url: "/Reports/delivered-scheduled-ratio/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "attendanceParticipation") {
                    return {
                        url: "/Reports/attendance-participation/chart",
                        params: {
                            courseIds: this.selectedCourseIds.join(",")
                        }
                    };
                }

                if (this.selectedReport === "dayOfWeekDelivered") {
                    return {
                        url: "/Reports/day-of-week-delivered/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }
                if (this.selectedReport === "deliveryFrequency") {
                    return {
                        url: "/Reports/training-delivery-frequency/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }
                if (this.selectedReport === "popularTrainingTop3") {
                    return {
                        url: "/Reports/popular-training-top3/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "averageAttendance") {
                    return {
                        url: "/Reports/average-attendance/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "cancelledByDay") {
                    return {
                        url: "/Reports/cancelled-by-day/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "trainingByMonth") {
                    return {
                        url: "/Reports/training-by-month/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }
                if (this.selectedReport === "trainerEngagement") {
                    return {
                        url: "/Reports/trainer-engagement/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                if (this.selectedReport === "trainingType") {
                    return {
                        url: "/Reports/training-type/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }
                if (this.selectedReport === "repeatParticipants") {
                    return {
                        url: "/Reports/repeat-participants/chart",
                        params: {
                            fromDate: this.fromDate,
                            toDate: this.toDate
                        }
                    };
                }

                return null;
            },

            async viewChart() {
                const requireCourses = this.selectedReport === "attendanceParticipation";
                const requireDates = this.selectedReport === "trainingsScheduled" ||
                    this.selectedReport === "deliveredScheduledRatio" ||
                    this.selectedReport === "dayOfWeekDelivered" ||
                    this.selectedReport === "deliveryFrequency" ||
                    this.selectedReport === "popularTrainingTop3" ||
                    this.selectedReport === "averageAttendance" ||
                    this.selectedReport === "cancelledByDay" ||
                    this.selectedReport === "cancelledTrainings" ||
                    this.selectedReport === "trainingByMonth" ||
                    this.selectedReport === "trainerEngagement" ||
                    this.selectedReport === "trainingType" ||
                    this.selectedReport === "repeatParticipants";
                if (!this.validateReportInputs(requireDates, requireCourses)) return;

                const config = this.getReportChartConfig();

                if (!config) {
                    alert("Chart for this report is not implemented yet.");
                    return;
                }

                try {
                    const res = await apiClient.get(config.url, {
                        params: config.params
                    });

                    const data = res.data?.$values ?? res.data ?? [];

                    if (this.selectedReport === "trainingsScheduled") {
                        this.chartTitle = "Trainings Scheduled by Month";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.month)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Scheduled Trainings",
                                data: data.map(x => x.count)
                            }
                        ];
                    }

                    if (this.selectedReport === "attendanceParticipation") {
                        this.chartTitle = "Attendance & Participation";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.courseTitle)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Registered",
                                data: data.map(x => x.registered)
                            },
                            {
                                name: "Attended",
                                data: data.map(x => x.attended)
                            }
                        ];
                    }

                    if (this.selectedReport === "popularTrainingTop3") {
                        this.chartTitle = "Most Popular Training - Top 3";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.training)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Total Attendance",
                                data: data.map(x => x.attendance)
                            }
                        ];
                    }

                    if (this.selectedReport === "deliveryFrequency") {
                        this.chartTitle = "Training Delivery by Frequency";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.training)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Delivered Count",
                                data: data.map(x => x.count)
                            }
                        ];
                    }

                    if (this.selectedReport === "deliveredScheduledRatio") {
                        this.chartTitle = "Delivered-to-Scheduled Ratio";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: ["Scheduled", "Delivered"]
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Courses",
                                data: [data.scheduled, data.delivered]
                            }
                        ];
                    }

                    if (this.selectedReport === "dayOfWeekDelivered") {
                        this.chartTitle = "Delivered Trainings by Day of Week";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.day)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Delivered Trainings",
                                data: data.map(x => x.count)
                            }
                        ];
                    }
                    if (this.selectedReport === "averageAttendance") {
                        this.chartTitle = "Top Trainings by Average Attendance";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.training)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Average Attendance",
                                data: data.map(x => x.average)
                            }
                        ];
                    }

                    if (this.selectedReport === "cancelledByDay") {
                        this.chartTitle = "Cancelled Trainings by Day of Week";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.day)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Cancelled Trainings",
                                data: data.map(x => x.count)
                            }
                        ];
                    }
                    if (this.selectedReport === "trainingByMonth") {
                        this.chartTitle = "Training by Month";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.month)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Scheduled",
                                data: data.map(x => x.scheduled)
                            },
                            {
                                name: "Delivered",
                                data: data.map(x => x.delivered)
                            },
                            {
                                name: "Cancelled",
                                data: data.map(x => x.cancelled)
                            }
                        ];
                    }

                    if (this.selectedReport === "trainerEngagement") {
                        this.chartTitle = "Trainer Engagement - Top 10 Trainers";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.trainer)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Total Trainings",
                                data: data.map(x => x.total)
                            },
                            {
                                name: "Delivered",
                                data: data.map(x => x.delivered)
                            },
                            {
                                name: "Cancelled",
                                data: data.map(x => x.cancelled)
                            }
                        ];
                    }

                    if (this.selectedReport === "trainingType") {
                        this.chartTitle = "Type of Training";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.type)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Trainings",
                                data: data.map(x => x.count)
                            }
                        ];
                    }
                    if (this.selectedReport === "repeatParticipants") {
                        this.chartTitle = "Repeat Participants - Top 10";

                        this.chartOptions = {
                            ...this.chartOptions,
                            xaxis: {
                                categories: data.map(x => x.participant)
                            }
                        };

                        this.chartSeries = [
                            {
                                name: "Courses Attended",
                                data: data.map(x => x.count)
                            }
                        ];
                    }

                    this.showChartModal = true;
                } catch (err) {
                    console.error("Chart load failed:", err);
                    alert(err?.response?.data?.message || "Failed to load chart.");
                }
            },

            async downloadReport(type) {
                const requireCourses = this.selectedReport === "attendanceParticipation";
                const requireDates = this.selectedReport === "trainingsScheduled" ||
                    this.selectedReport === "deliveredScheduledRatio" ||
                    this.selectedReport === "dayOfWeekDelivered" ||
                    this.selectedReport === "deliveryFrequency" ||
                    this.selectedReport === "popularTrainingTop3" ||
                    this.selectedReport === "averageAttendance" ||
                    this.selectedReport === "cancelledByDay" ||
                    this.selectedReport === "cancelledTrainings" ||
                    this.selectedReport === "trainingByMonth" ||
                    this.selectedReport === "trainerEngagement" ||
                    this.selectedReport === "trainingType" ||
                    this.selectedReport === "repeatParticipants";

                if (!this.validateReportInputs(requireDates, requireCourses)) return;

                const config = this.getReportDownloadConfig(type);

                if (!config) {
                    alert("This report is not implemented yet.");
                    return;
                }

                try {
                    const res = await apiClient.get(config.url, {
                        params: config.params,
                        responseType: "blob"
                    });

                    const contentType = res.headers["content-type"];

                    if (contentType && contentType.includes("application/json")) {
                        const text = await res.data.text();
                        const json = JSON.parse(text);
                        alert(json.message || "Report download failed.");
                        return;
                    }

                    const blob = new Blob([res.data], {
                        type: type === "pdf" ? "application/pdf" : "text/csv"
                    });

                    const blobUrl = window.URL.createObjectURL(blob);
                    const link = document.createElement("a");

                    link.href = blobUrl;
                    link.download = config.fileName;

                    document.body.appendChild(link);
                    link.click();

                    setTimeout(() => {
                        link.remove();
                        window.URL.revokeObjectURL(blobUrl);
                    }, 100);
                } catch (err) {
                    console.error("Report download failed:", err);

                    if (err?.response?.data instanceof Blob) {
                        const text = await err.response.data.text();

                        try {
                            const json = JSON.parse(text);
                            alert(json.message || "Failed to download report.");
                        } catch {
                            alert(text || "Failed to download report.");
                        }

                        return;
                    }

                    alert(err?.response?.data?.message || "Failed to download report.");
                }
            },

            toggleSelectAll(e) {
                if (e.target.checked) {
                    const visibleIds = this.filteredCourses.map(c => c.courseSysId);
                    this.selectedCourseIds = [...new Set([...this.selectedCourseIds, ...visibleIds])];
                } else {
                    const visibleIds = this.filteredCourses.map(c => c.courseSysId);
                    this.selectedCourseIds = this.selectedCourseIds.filter(id => !visibleIds.includes(id));
                }
            },

            resetAll() {
                this.fromDate = "";
                this.toDate = "";
                this.searchQuery = "";
                this.selectedCourseIds = [];
                this.selectedReport = "";
                this.showChartModal = false;
                this.chartSeries = [];
            },

            formatDate(date) {
                if (!date) return "N/A";
                return new Date(date).toLocaleDateString();
            },
        },
    };</script>

<style scoped>
    .reports-page {
        padding: 28px;
        min-height: 100vh;
        background: radial-gradient(circle at top left, rgba(67, 40, 93, 0.08), transparent 35%), linear-gradient(180deg, #f8fafc 0%, #eef2f7 100%);
    }

        .reports-page h2 {
            color: #111827;
            font-size: 30px;
            font-weight: 800;
            margin: 0 0 20px;
        }

    /* Date row */
    .date-card {
        background: #ffffff;
        border: 1px solid #e5e7eb;
        border-radius: 20px;
        padding: 20px;
        margin-bottom: 22px;
        display: grid;
        grid-template-columns: 220px 220px auto;
        gap: 18px;
        align-items: end;
        box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08);
    }

    .filter-field {
        display: flex;
        flex-direction: column;
        gap: 6px;
    }

        .filter-field label {
            font-weight: 700;
            color: #374151;
        }

        .filter-field input,
        .table-search input {
            height: 42px;
            padding: 9px 14px;
            border-radius: 999px;
            border: 1px solid #d1d5db;
            font-size: 15px;
            background: #fff;
            box-sizing: border-box;
        }

            .filter-field input:focus,
            .table-search input:focus {
                outline: none;
                border-color: #43285D;
                box-shadow: 0 0 0 3px rgba(67, 40, 93, 0.12);
            }

    .reset-btn {
        width: 120px;
        height: 42px;
        border: none;
        border-radius: 999px;
        background: #43285D;
        color: white;
        font-weight: 700;
        cursor: pointer;
    }

        .reset-btn:hover {
            background: #361F4A;
        }

    /* Course selection card */
    .course-select-card {
        background: white;
        border-radius: 20px;
        padding: 22px;
        box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08);
        border: 1px solid #e5e7eb;
    }

    .course-list-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 14px;
    }

        .course-list-header h3 {
            margin: 0;
            color: #43285D;
            font-size: 22px;
            font-weight: 800;
        }

        .course-list-header p {
            margin: 4px 0 0;
            color: #6b7280;
        }

    .select-all {
        display: flex;
        align-items: center;
        gap: 8px;
        font-weight: 700;
        color: #43285D;
    }

    .table-search {
        margin-bottom: 14px;
    }

        .table-search input {
            width: 360px;
            max-width: 100%;
        }

    /* Fixed table container */
    .course-table-wrapper {
        height: 430px;
        overflow-y: auto;
        border: 1px solid #e5e7eb;
        border-radius: 16px;
        background: #ffffff;
    }

        .course-table-wrapper table {
            width: 100%;
            border-collapse: collapse;
            table-layout: fixed;
        }

        .course-table-wrapper thead {
            position: sticky;
            top: 0;
            z-index: 5;
            background: #f4eff9;
        }

        .course-table-wrapper th {
            padding: 14px 12px;
            text-align: left;
            color: #43285D;
            font-weight: 800;
            border-bottom: 1px solid #e5e7eb;
        }

        .course-table-wrapper td {
            padding: 13px 12px;
            border-bottom: 1px solid #eef2f7;
            color: #111827;
            vertical-align: top;
            word-break: break-word;
        }

        .course-table-wrapper tbody tr:hover {
            background: #faf7fd;
        }

        .course-table-wrapper th:first-child,
        .course-table-wrapper td:first-child {
            width: 44px;
            text-align: center;
        }

    @media (max-width: 900px) {
        .date-card {
            grid-template-columns: 1fr;
        }

        .reset-btn {
            width: 100%;
        }
    }
    .report-options-card {
        background: white;
        border-radius: 20px;
        padding: 22px;
        margin-top: 22px;
        box-shadow: 0 12px 30px rgba(15, 23, 42, 0.08);
        border: 1px solid #e5e7eb;
    }

    .report-options-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 16px;
        margin-bottom: 14px;
    }

        .report-options-header h3 {
            margin: 0;
            color: #43285D;
            font-size: 22px;
            font-weight: 800;
        }

        .report-options-header p {
            margin: 4px 0 0;
            color: #6b7280;
        }

    .download-actions {
        display: flex;
        gap: 10px;
    }

    .download-btn {
        height: 40px;
        padding: 0 18px;
        border: none;
        border-radius: 999px;
        font-weight: 700;
        cursor: pointer;
    }

    .pdf-btn {
        background: #43285D;
        color: white;
    }

    .csv-btn {
        background: #eef2f7;
        color: #111827;
        border: 1px solid #d1d5db;
    }

    .report-table-wrapper {
        max-height: 360px;
        overflow-y: auto;
        border: 1px solid #e5e7eb;
        border-radius: 16px;
    }

        .report-table-wrapper table {
            width: 100%;
            border-collapse: collapse;
            table-layout: fixed;
        }

        .report-table-wrapper thead {
            position: sticky;
            top: 0;
            background: #f4eff9;
            z-index: 5;
        }

        .report-table-wrapper th,
        .report-table-wrapper td {
            padding: 14px 12px;
            border-bottom: 1px solid #eef2f7;
            text-align: left;
        }

        .report-table-wrapper th {
            color: #43285D;
            font-weight: 800;
        }

            .report-table-wrapper th:first-child,
            .report-table-wrapper td:first-child {
                width: 50px;
                text-align: center;
            }

        .report-table-wrapper tbody tr {
            cursor: pointer;
        }

            .report-table-wrapper tbody tr:hover,
            .report-table-wrapper tbody tr.selected {
                background: #faf7fd;
            }
    .report-options-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-end;
        gap: 18px;
        margin-bottom: 14px;
        flex-wrap: wrap;
    }

    .report-header-actions {
        display: flex;
        align-items: flex-end;
        gap: 12px;
        flex-wrap: wrap;
    }

    .small-date {
        min-width: 160px;
    }

    .reset-btn {
        width: 100px;
        height: 42px;
        border: none;
        border-radius: 999px;
        background: #43285D;
        color: white;
        font-weight: 700;
        cursor: pointer;
    }

        .reset-btn:hover {
            background: #361F4A;
        }

    .download-actions {
        display: flex;
        gap: 10px;
    }
    .chart-btn {
        background: #f4eff9;
        color: #43285D;
        border: 1px solid #cbb8dd;
    }

        .chart-btn:hover {
            background: #eadff3;
        }

    .chart-modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(15, 23, 42, 0.65);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 2000;
        padding: 24px;
    }

    .chart-modal {
        background: white;
        width: 850px;
        max-width: 100%;
        border-radius: 22px;
        padding: 24px;
        box-shadow: 0 24px 60px rgba(15, 23, 42, 0.25);
    }

    .chart-modal-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 18px;
    }

        .chart-modal-header h3 {
            margin: 0;
            color: #43285D;
            font-size: 24px;
            font-weight: 800;
        }

        .chart-modal-header p {
            margin: 4px 0 0;
            color: #6b7280;
        }

    .chart-close {
        width: 38px;
        height: 38px;
        border-radius: 50%;
        border: none;
        background: #43285D;
        color: white;
        font-size: 24px;
        cursor: pointer;
    }
</style>