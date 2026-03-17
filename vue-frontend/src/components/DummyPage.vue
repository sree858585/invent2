<template>
    <div class="home-page-container">
        <template v-if="!isSideNavRendered">
            <SideNav :isUserLoggedIn="isUserLoggedIn" />
        </template>

        <section class="hero-header">
            <div class="hero-inner">
                <span class="hero-kicker">New York State Department of Health AIDS Institute</span>

                <h1 class="hero-title">
                    Welcome to the AIDS Institute Education and Training Website!
                </h1>

                <p class="hero-intro">
                    Progressive continuing education for non-physician health and human services providers across New York State.
                </p>
            </div>
        </section>

        <section class="hero-carousel-section">
            <div v-if="loading" class="carousel-state-card">
                Loading banners...
            </div>

            <div v-else-if="images.length === 0" class="carousel-state-card empty">
                No active banners are available right now.
            </div>

            <template v-else>
                <div class="hero-carousel">
                    <button v-if="images.length > 1"
                            class="hero-arrow left"
                            @click="prevImage"
                            aria-label="Previous banner">
                        ❮
                    </button>

                    <div class="hero-track">
                        <button v-for="(img, i) in images"
                                :key="img.homeBannerSysId || i"
                                type="button"
                                class="hero-card"
                                :class="{
                                active: i === currentImageIndex,
                                prev: i === prevIndex && images.length>
                            1,
                            next: i === nextIndex && images.length > 1,
                            single: images.length === 1
                            }"
                            @click="handleBannerClick(img)"
                            >
                            <img :src="img.src" :alt="img.alt || img.bannerName || 'Home banner'" />

                            <div class="hero-overlay"></div>

                            <div class="hero-content">
                                <span class="hero-type" :class="img.actionType?.toLowerCase()">
                                    {{ img.actionType }}
                                </span>

                                <h3>{{ img.bannerName }}</h3>

                                <p v-if="img.modalTitle">
                                    {{ img.modalTitle }}
                                </p>
                            </div>
                        </button>
                    </div>

                    <button v-if="images.length > 1"
                            class="hero-arrow right"
                            @click="nextImage"
                            aria-label="Next banner">
                        ❯
                    </button>
                </div>

                <div v-if="images.length > 1" class="hero-dots">
                    <span v-for="(img, i) in images"
                          :key="img.homeBannerSysId || `dot-${i}`"
                          class="hero-dot"
                          :class="{ active: i === currentImageIndex }"
                          @click="goToImage(i)"></span>
                </div>
            </template>
        </section>

        <section class="home-content-section">
            <div class="content-shell">
                <div class="content-main-card">
                    <div class="content-heading-row">
                        <div>
                            <span class="section-chip">About the Program</span>
                            <h2>AIDS Institute Education and Training</h2>
                        </div>
                    </div>

                    <div class="content-body">
                        <p>
                            New York State Education and Training is a New York State Department of Health AIDS Institute
                            program that offers progressive continuing education intended for
                            <strong>non-physician health and human services providers</strong>
                            on topics such as HIV Care and Prevention, Sexual Health, Hepatitis C treatment and prevention,
                            LGBTQIA+ health, Harm Reduction, Drug User Health, Social Determinants of Health, and many other topics.
                        </p>

                        <p>
                            Our trainings are designed to fit your schedule. We offer both live and on-demand virtual sessions
                            as well as in-person trainings. <strong>All trainings are free of charge.</strong>
                        </p>
                    </div>
                </div>

                <div class="content-grid">
                    <div class="content-card spotlight-card">
                        <span class="section-chip soft">Important</span>
                        <h3>Course Registration Information</h3>
                        <p>
                            Once you have registered for a course, you will receive a confirmation email with course details.
                            This email can sometimes end up in a junk or spam folder, so please check those folders if you do
                            not see it in your inbox.
                        </p>
                        <p>
                            You can also find all your course information by going to <strong>My Courses</strong>.
                            You must be logged into your account to access that page.
                        </p>
                    </div>

                    <div class="content-card link-card">
                        <span class="section-chip soft">Featured</span>
                        <h3>New York State Certified Peer Worker Program</h3>
                        <p>
                            Find information about the New York State Certified Peer Worker program.
                        </p>
                        <button class="content-link-btn"
                                type="button"
                                @click="goToPeerCertification">
                            Learn More
                        </button>
                    </div>

                    <div class="content-card link-card">
                        <span class="section-chip soft">Clinical Providers</span>
                        <h3>Clinical Education Initiative (CEI)</h3>
                        <p>
                            The Clinical Education Initiative offers trainings for clinical providers.
                        </p>
                        <a href="https://ceitraining.org/"
                           target="_blank"
                           rel="noopener noreferrer"
                           class="content-link-btn link-anchor">
                            Visit CEI Training
                        </a>
                    </div>
                </div>
            </div>
        </section>
        <div v-if="selectedCourse" style="padding:10px; background:#d1fae5; margin-top:20px;">
            selectedCourse loaded: {{ selectedCourse.courseSysId }}
        </div>
        <CourseDetailModal v-if="selectedCourse"
                           :course="selectedCourse"
                           @register="handleRegister"
                           @request-login="showLoginModal = true"
                           @close="selectedCourse = null" />

        <SuccessModal v-if="showSuccessModal"
                      :message="successMessage"
                      :email="user?.email || ''"
                      @close="showSuccessModal = false" />

        <LoginComponent v-if="showLoginModal"
                        @login-success="handleLoginSuccess"
                        @close="showLoginModal = false"
                        @show-register="handleShowRegister" />

        <RegisterComponent v-if="showRegisterModal"
                           @close="showRegisterModal = false"
                           @register-success="handleRegisterSuccess" />
        <InfoBannerModal v-if="selectedInfoBanner"
                         :banner="selectedInfoBanner"
                         @close="selectedInfoBanner = null" />
    </div>
</template>
<script>import apiClient from "@/axios";
    import SideNav from "@/components/SideNav.vue";
    import CourseDetailModal from "@/components/Modals/CourseDetailModal.vue";
    import SuccessModal from "@/components/Modals/SuccessModal.vue";
    import LoginComponent from "@/components/LoginComponent.vue";
    import RegisterComponent from "@/components/RegistrationModal.vue";
    import InfoBannerModal from "@/components/Modals/InfoBannerModal.vue";

    export default {
        name: "HomePage",
        components: {
            SideNav,
            CourseDetailModal,
            SuccessModal,
            LoginComponent,
            RegisterComponent,
            InfoBannerModal

        },
        data() {
            return {
                userName: localStorage.getItem("userName") || null,
                isUserLoggedIn: !!localStorage.getItem("jwtToken"),
                isSideNavRendered: false,

                currentImageIndex: 0,
                currentOffset: 0,
                carouselInterval: null,
                loading: false,

                images: [],
                selectedBanner: null,
                selectedInfoBanner: null,

                selectedCourse: null,
                user: null,
                showSuccessModal: false,
                successMessage: "",
                showLoginModal: false,
                showRegisterModal: false
            };
        },

        computed: {
            prevIndex() {
                if (this.images.length <= 1) return 0;
                return (this.currentImageIndex - 1 + this.images.length) % this.images.length;
            },
            nextIndex() {
                if (this.images.length <= 1) return 0;
                return (this.currentImageIndex + 1) % this.images.length;
            },
        },

        async mounted() {
            this.isSideNavRendered = true;
            await this.loadHomeBanners();
            this.startImageCarousel();
            this.fetchUser();
        },

        beforeUnmount() {
            this.stopImageCarousel();
        },

        methods: {
            async loadHomeBanners() {
                this.loading = true;
                try {
                    const res = await apiClient.get("/HomeBanner/active");
                    const rows = Array.isArray(res.data) ? res.data : (res.data?.$values || []);

                    this.images = rows
                        .filter(x => x.imageUrl)
                        .map(x => ({
                            ...x,
                            src: this.fullImageUrl(x.imageUrl),
                            alt: x.bannerName || "Home banner",
                        }));

                    if (this.currentImageIndex >= this.images.length) {
                        this.currentImageIndex = 0;
                    }
                } catch (err) {
                    console.error("Failed to load home banners:", err);
                    this.images = [];
                } finally {
                    this.loading = false;
                }
            },
            goToPeerCertification() {
                this.$router.push("/peer-certification");
            },

            async fetchUser() {
                const userId = localStorage.getItem("userId");
                if (!userId) return;

                try {
                    const res = await apiClient.get(`/user/${userId}`);
                    this.user = res.data;
                } catch (err) {
                    console.error("Failed to fetch user:", err?.response?.data || err);
                }
            },

            fullImageUrl(url) {
                if (!url) return "";

                if (url.startsWith("http://") || url.startsWith("https://")) {
                    return url;
                }

                const base = apiClient.defaults.baseURL || "";
                const cleanedBase = base.endsWith("/api")
                    ? base.replace(/\/api$/, "")
                    : base;

                return `${cleanedBase}${url}`;
            },

            startImageCarousel() {
                this.stopImageCarousel();

                if (this.images.length <= 1) return;

                this.carouselInterval = setInterval(() => {
                    this.nextImage();
                }, 5000);
            },

            stopImageCarousel() {
                if (this.carouselInterval) {
                    clearInterval(this.carouselInterval);
                    this.carouselInterval = null;
                }
            },

            nextImage() {
                if (this.images.length <= 1) return;
                this.currentImageIndex = (this.currentImageIndex + 1) % this.images.length;
            },

            prevImage() {
                if (this.images.length <= 1) return;
                this.currentImageIndex =
                    (this.currentImageIndex - 1 + this.images.length) % this.images.length;
            },

            goToImage(i) {
                this.currentImageIndex = i;
            },

            async handleBannerClick(banner) {
                console.log("clicked banner:", banner);

                this.selectedBanner = banner;

                if (banner.actionType === "Course" && banner.courseSysId) {
                    try {
                        const res = await apiClient.get(`/Course/${banner.courseSysId}`);

                        console.log("course details api response:", res.data);

                        this.selectedCourse = res.data;

                        console.log("selectedCourse after set:", this.selectedCourse);

                        await this.$nextTick();
                        console.log("modal should render now");
                    } catch (err) {
                        console.error("Failed to load course details:", err?.response?.data || err);
                        alert("Failed to load course details.");
                    }
                    return;
                }

                if (banner.actionType === "Info") {
                    this.selectedInfoBanner = banner;
                    return;
                }
            },

            handleShowRegister() {
                this.showLoginModal = false;
                this.showRegisterModal = true;
            },

            handleRegisterSuccess() {
                this.showRegisterModal = false;
                this.fetchUser();
            },

            handleLoginSuccess(userData) {
                localStorage.setItem("userId", userData.userId);
                localStorage.setItem("userName", `${userData.firstName} ${userData.lastName}`);
                localStorage.setItem("jwtToken", userData.token);

                this.userName = `${userData.firstName} ${userData.lastName}`;
                this.isUserLoggedIn = true;
                this.showLoginModal = false;

                this.fetchUser();

                if (this.selectedCourse) {
                    this.handleRegister(this.selectedCourse, true);
                }
            },

            async handleRegister(course, isFromLogin = false) {
                try {
                    const userId = localStorage.getItem("userId");

                    if (!userId) {
                        this.showLoginModal = true;
                        return;
                    }

                    const res = await apiClient.post("/Course/register", {
                        userId,
                        courseId: course.courseSysId,
                        adaneed: course.adaneed || false,
                        adadetails: course.adadetails || "",
                    });

                    this.successMessage = res.data?.message || "Registration successful.";
                    this.showSuccessModal = true;

                    if (!isFromLogin) {
                        this.selectedCourse = null;
                    }
                } catch (err) {
                    console.error("Registration failed:", err?.response?.data || err);
                    alert(err?.response?.data?.message || "Registration failed.");
                }
            },
        },

        watch: {
            images() {
                this.startImageCarousel();
            }
        }
    };</script>

<style scoped>
    .home-page-container {
        font-family: "Segoe UI", "Inter", Arial, sans-serif;
        line-height: 1.6;
        color: #1f2937;
        padding: 28px;
        position: relative;
        background: radial-gradient(circle at top left, rgba(126, 34, 206, 0.06), transparent 30%), radial-gradient(circle at top right, rgba(236, 72, 153, 0.07), transparent 28%), linear-gradient(180deg, #f8fafc 0%, #f4f7fb 100%);
        min-height: 100vh;
    }

    /* HEADER */
    .header-section {
        max-width: 800px;
        margin: 0 auto 26px;
        padding: 22px 10px 6px;
        text-align: center;
    }

    .hero-copy {
        max-width: 900px;
        margin: 0 auto;
    }

    .hero-kicker {
        display: inline-block;
        margin-bottom: 10px;
        padding: 7px 14px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.08);
        color: #5b3a7a;
        font-size: 13px;
        font-weight: 700;
        letter-spacing: 0.02em;
    }

    .header-section h1 {
        margin: 0;
        font-size: clamp(2rem, 4vw, 3.25rem);
        line-height: 1.08;
        font-weight: 800;
        color: #241833;
    }

    .hero-description {
        margin: 16px auto 0;
        max-width: 760px;
        font-size: 1.05rem;
        color: #5b6472;
    }

    /* CAROUSEL SECTION */
    .hero-carousel-section {
        max-width: 1200px;
        margin: 0 auto 40px;
    }

    .carousel-state-card {
        min-height: 360px;
        border-radius: 28px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: white;
        border: 1px solid #e6eaf0;
        box-shadow: 0 18px 40px rgba(15, 23, 42, 0.08);
        color: #6b7280;
        font-size: 16px;
        font-weight: 600;
    }

        .carousel-state-card.empty {
            background: linear-gradient(135deg, #ffffff 0%, #f7f8fc 100%);
        }

    /* HERO CAROUSEL */
    .hero-carousel {
        width: 100%;
        max-width: 1160px;
        height: 470px;
        margin: 0 auto;
        display: flex;
        align-items: center;
        position: relative;
    }

    .hero-track {
        width: 100%;
        height: 100%;
        position: relative;
        display: flex;
        justify-content: center;
        overflow: visible;
    }

    .hero-card {
        position: absolute;
        width: 720px;
        height: 420px;
        border-radius: 30px;
        overflow: hidden;
        opacity: 0;
        transform: scale(0.76) translateX(0);
        transition: transform 0.6s ease, opacity 0.6s ease, box-shadow 0.35s ease, filter 0.35s ease;
        box-shadow: 0 22px 45px rgba(15, 23, 42, 0.18);
        border: none;
        padding: 0;
        background: #fff;
        cursor: pointer;
    }

        .hero-card img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
        }

        /* Center card */
        .hero-card.active {
            opacity: 1;
            transform: scale(1) translateX(0);
            z-index: 5;
            filter: saturate(1.03);
        }

        /* Left preview */
        .hero-card.prev {
            opacity: 0.62;
            transform: scale(0.82) translateX(-360px);
            z-index: 3;
            filter: blur(0.2px) saturate(0.9);
        }

        /* Right preview */
        .hero-card.next {
            opacity: 0.62;
            transform: scale(0.82) translateX(360px);
            z-index: 3;
            filter: blur(0.2px) saturate(0.9);
        }

        .hero-card.single {
            opacity: 1;
            transform: scale(1) translateX(0);
            z-index: 5;
            cursor: default;
        }

        .hero-card:hover.active {
            transform: scale(1.015) translateX(0);
            box-shadow: 0 30px 60px rgba(15, 23, 42, 0.22);
        }

    .hero-overlay {
        position: absolute;
        inset: 0;
        background: linear-gradient(180deg, rgba(17, 24, 39, 0.10) 0%, rgba(17, 24, 39, 0.18) 35%, rgba(17, 24, 39, 0.72) 100%);
        z-index: 1;
    }

    .hero-content {
        position: absolute;
        left: 28px;
        right: 28px;
        bottom: 28px;
        z-index: 2;
        color: white;
        text-align: left;
    }

        .hero-content h3 {
            margin: 0 0 8px;
            font-size: clamp(1.4rem, 2.5vw, 2.2rem);
            line-height: 1.12;
            font-weight: 800;
            text-shadow: 0 4px 14px rgba(0, 0, 0, 0.35);
        }

        .hero-content p {
            margin: 0;
            font-size: 0.98rem;
            opacity: 0.95;
            max-width: 70%;
            text-shadow: 0 3px 10px rgba(0, 0, 0, 0.3);
        }

    .hero-type {
        display: inline-flex;
        align-items: center;
        margin-bottom: 14px;
        padding: 8px 14px;
        border-radius: 999px;
        font-size: 12px;
        font-weight: 800;
        letter-spacing: 0.02em;
        backdrop-filter: blur(6px);
    }

        .hero-type.info {
            background: rgba(168, 85, 247, 0.2);
            border: 1px solid rgba(216, 180, 254, 0.45);
            color: #f3e8ff;
        }

        .hero-type.course {
            background: rgba(34, 197, 94, 0.18);
            border: 1px solid rgba(187, 247, 208, 0.4);
            color: #dcfce7;
        }

    /* ARROWS */
    .hero-arrow {
        width: 58px;
        height: 58px;
        border-radius: 50%;
        border: none;
        background: rgba(255, 255, 255, 0.95);
        box-shadow: 0 12px 28px rgba(0, 0, 0, 0.18);
        font-size: 28px;
        cursor: pointer;
        position: absolute;
        top: 50%;
        transform: translateY(-50%);
        z-index: 10;
        transition: all 0.25s ease;
        color: #3b2a52;
    }

        .hero-arrow.left {
            left: -20px;
        }

        .hero-arrow.right {
            right: -20px;
        }

        .hero-arrow:hover {
            background: white;
            transform: translateY(-50%) scale(1.06);
            box-shadow: 0 16px 30px rgba(0, 0, 0, 0.22);
        }

    /* DOTS */
    .hero-dots {
        text-align: center;
        margin-top: 22px;
    }

    .hero-dot {
        width: 11px;
        height: 11px;
        background: #c5cad3;
        display: inline-block;
        margin: 0 6px;
        border-radius: 50%;
        cursor: pointer;
        transition: all 0.28s ease;
    }

        .hero-dot.active {
            width: 28px;
            border-radius: 999px;
            background: linear-gradient(90deg, #6d28d9, #ec4899);
        }

    /* INFO CARDS */
    .info-grid {
        max-width: 1200px;
        margin: 10px auto 0;
        display: grid;
        grid-template-columns: 1.25fr 1fr 1fr;
        gap: 22px;
    }

    .info-card,
    .important-info-section {
        background: rgba(255, 255, 255, 0.88);
        border: 1px solid rgba(226, 232, 240, 0.95);
        border-radius: 24px;
        padding: 24px 24px;
        box-shadow: 0 16px 35px rgba(15, 23, 42, 0.07);
        backdrop-filter: blur(8px);
    }

    .highlight {
        background: linear-gradient(180deg, rgba(255,255,255,0.98) 0%, rgba(248,245,255,0.96) 100%);
    }

    .important-info-section h2 {
        margin-top: 0;
        margin-bottom: 12px;
        font-size: 1.35rem;
        color: #2d1f3d;
        line-height: 1.2;
    }

    .info-card p,
    .important-info-section p {
        margin: 0 0 14px;
        color: #4b5563;
        font-size: 15px;
    }

        .info-card p:last-child,
        .important-info-section p:last-child {
            margin-bottom: 0;
        }

    .info-card a {
        color: #6d28d9;
        font-weight: 700;
        text-decoration: none;
    }

        .info-card a:hover {
            text-decoration: underline;
        }

    /* RESPONSIVE */
    @media (max-width: 1200px) {
        .hero-card {
            width: 640px;
            height: 390px;
        }

            .hero-card.prev {
                transform: scale(0.82) translateX(-300px);
            }

            .hero-card.next {
                transform: scale(0.82) translateX(300px);
            }

        .info-grid {
            grid-template-columns: 1fr;
        }
    }

    @media (max-width: 900px) {
        .home-page-container {
            padding: 18px;
        }

        .hero-carousel {
            height: 360px;
        }

        .hero-card {
            width: 100%;
            max-width: 720px;
            height: 320px;
        }

            .hero-card.prev,
            .hero-card.next {
                opacity: 0;
                pointer-events: none;
            }

        .hero-arrow.left {
            left: 8px;
        }

        .hero-arrow.right {
            right: 8px;
        }

        .hero-content {
            left: 20px;
            right: 20px;
            bottom: 20px;
        }

            .hero-content p {
                max-width: 100%;
            }
    }

    @media (max-width: 640px) {
        .header-section h1 {
            font-size: 2rem;
        }

        .hero-carousel {
            height: 290px;
        }

        .hero-card {
            height: 250px;
            border-radius: 22px;
        }

        .hero-arrow {
            width: 46px;
            height: 46px;
            font-size: 22px;
        }

        .important-info-section h2 {
            font-size: 1.15rem;
        }
    }
    .hero-header {
        max-width: 1280px;
        margin: 0 auto 10px;
        padding: 4px 8px 0;
        text-align: center;
    }

    .hero-inner {
        max-width: 900px;
        margin: 0 auto;
    }

    .hero-title {
        margin: 0;
        font-size: clamp(1.8rem, 3.3vw, 2.95rem);
        line-height: 1.08;
        font-weight: 750;
        letter-spacing: -0.03em;
        color: #201132;
        max-width: 860px;
        margin-inline: auto;
    }

    .hero-subline {
        display: block;
        margin-top: 4px;
        font-size: 0.95em;
        font-weight: 700;
    }

    .hero-highlight {
        display: inline;
        background: linear-gradient(90deg, #5b21b6 0%, #7c3aed 45%, #db2777 100%);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        background-clip: text;
    }
    .hero-header {
        max-width: 1280px;
        margin: 0 auto 18px;
        padding: 8px 8px 0;
        text-align: center;
    }

    .hero-inner {
        max-width: 980px;
        margin: 0 auto;
    }

    .hero-kicker {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        margin-bottom: 12px;
        padding: 8px 16px;
        border-radius: 999px;
        background: rgba(67, 40, 93, 0.08);
        color: #43285D;
        font-size: 12px;
        font-weight: 700;
        letter-spacing: 0.03em;
        border: 1px solid rgba(67, 40, 93, 0.08);
    }

    .hero-title {
        margin: 0;
        font-size: clamp(2rem, 3.5vw, 3.25rem);
        line-height: 1.08;
        font-weight: 800;
        letter-spacing: -0.03em;
        color: #201132;
        max-width: 920px;
        margin-inline: auto;
    }

    .hero-intro {
        margin: 14px auto 0;
        max-width: 760px;
        font-size: 1.05rem;
        line-height: 1.7;
        color: #5b6472;
    }

    /* CONTENT SECTION */
    .home-content-section {
        max-width: 1240px;
        margin: 10px auto 0;
    }

    .content-shell {
        display: flex;
        flex-direction: column;
        gap: 24px;
    }

    .content-main-card {
        background: linear-gradient(180deg, rgba(255,255,255,0.98) 0%, rgba(250,248,255,0.98) 100%);
        border: 1px solid #e8eaf2;
        border-radius: 28px;
        padding: 34px 34px;
        box-shadow: 0 18px 38px rgba(15, 23, 42, 0.07);
    }

    .content-heading-row {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 20px;
        margin-bottom: 14px;
    }

    .content-main-card h2 {
        margin: 0;
        font-size: 2rem;
        line-height: 1.15;
        color: #201132;
        font-weight: 800;
        letter-spacing: -0.02em;
    }

    .content-body {
        max-width: 1000px;
    }

        .content-body p {
            margin: 0 0 18px;
            font-size: 1.08rem;
            line-height: 1.9;
            color: #414b5a;
        }

            .content-body p:last-child {
                margin-bottom: 0;
            }

    .section-chip {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        margin-bottom: 12px;
        padding: 7px 14px;
        border-radius: 999px;
        background: #43285D;
        color: #ffffff;
        font-size: 11px;
        font-weight: 700;
        letter-spacing: 0.05em;
        text-transform: uppercase;
    }

        .section-chip.soft {
            background: rgba(67, 40, 93, 0.10);
            color: #43285D;
        }

    .content-grid {
        display: grid;
        grid-template-columns: 1.25fr 1fr 1fr;
        gap: 22px;
    }

    .content-card {
        background: rgba(255, 255, 255, 0.96);
        border: 1px solid #e7eaf1;
        border-radius: 24px;
        padding: 28px 24px;
        box-shadow: 0 14px 32px rgba(15, 23, 42, 0.06);
        transition: transform 0.25s ease, box-shadow 0.25s ease;
    }

        .content-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 18px 36px rgba(15, 23, 42, 0.10);
        }

        .content-card h3 {
            margin: 0 0 12px;
            font-size: 1.35rem;
            line-height: 1.25;
            color: #201132;
            font-weight: 750;
        }

        .content-card p {
            margin: 0 0 14px;
            color: #556071;
            font-size: 15px;
            line-height: 1.8;
        }

            .content-card p:last-child {
                margin-bottom: 0;
            }

    .spotlight-card {
        background: linear-gradient(180deg, #ffffff 0%, #faf7ff 100%);
        border-color: #e7def6;
    }

    .link-card {
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }

    .content-link-btn {
        margin-top: 10px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: fit-content;
        min-width: 140px;
        padding: 11px 18px;
        border-radius: 999px;
        border: none;
        background: #43285D;
        color: white;
        font-size: 14px;
        font-weight: 700;
        text-decoration: none;
        cursor: pointer;
        box-shadow: 0 8px 18px rgba(67, 40, 93, 0.22);
        transition: all 0.22s ease;
    }

        .content-link-btn:hover {
            background: #361F4A;
            transform: translateY(-2px);
            box-shadow: 0 12px 22px rgba(67, 40, 93, 0.28);
        }

    .link-anchor {
        text-decoration: none;
    }

    /* MOBILE */
    @media (max-width: 1100px) {
        .content-grid {
            grid-template-columns: 1fr;
        }
    }

    @media (max-width: 768px) {
        .hero-title {
            font-size: 2rem;
        }

        .hero-intro {
            font-size: 0.98rem;
        }

        .content-main-card,
        .content-card {
            padding: 22px 18px;
        }

            .content-main-card h2 {
                font-size: 1.6rem;
            }

        .content-body p {
            font-size: 1rem;
            line-height: 1.8;
        }
    }
</style>