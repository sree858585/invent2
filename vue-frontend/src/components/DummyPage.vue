<template>
    <div class="home-page-container">
        <!-- Ensure SideNav is only rendered once -->
        <template v-if="!isSideNavRendered">
            <SideNav :isUserLoggedIn="isUserLoggedIn" />
        </template>

        <!-- Header Section -->
        <div class="header-section">
            <h1>Welcome to the Online Registration System for NYS AIDS Institute Training Centers</h1>
            <!-- <h1 v-else>Hello, {{ userName }}</h1> -->
            <!-- <p><strong>Please Note:</strong> The preferred web browser ...</p> -->
        </div>

        <!-- MODERN HERO CAROUSEL -->
        <div class="hero-carousel">

            <!-- Left Arrow -->
            <button class="hero-arrow left" @click="prevImage">❮</button>

            <!-- Track -->
            <div class="hero-track">
                <div v-for="(img, i) in images"
                     :key="i"
                     class="hero-card"
                     :class="{
                active: i === currentImageIndex,
                prev: i === prevIndex,
                next: i === nextIndex
             }">
                    <img :src="img.src" :alt="img.alt" />
                </div>
            </div>

            <!-- Right Arrow -->
            <button class="hero-arrow right" @click="nextImage">❯</button>

        </div>

        <!-- Dots -->
        <div class="hero-dots">
            <span v-for="(img, i) in images"
                  :key="i"
                  class="hero-dot"
                  :class="{ active: i === currentImageIndex }"
                  @click="goToImage(i)"></span>
        </div>

        <!-- Login / Logout Buttons -->
        <!--<button v-if="!userName" class="register-btn" @click="showLoginModal = true">Login</button>
    <button v-if="userName" class="register-btn logout-btn" @click="handleLogout">Logout</button>-->
        <!-- Modals -->
        <!--<LoginComponent v-if="showLoginModal"
                    @login-success="handleLoginSuccess"
                    @show-register="handleShowRegister"
                    @close="showLoginModal = false" />
    <RegistrationModal v-if="showRegistrationForm"
                       @close="showRegistrationForm = false" />-->
        <!-- Important Information Section -->
        <div class="important-info-section">
            <h2>Important Course Registration Information:</h2>
            <p>
                Once you have registered for a course, you will receive a confirmation email with details about the course.
                This email can sometimes end up in a junk or spam folder, so please check those places if you do not see it in
                your inbox. You can also find all of your course information by going to ‘My Courses’ on the right-hand column
                of this page. You must be logged into your account to access the ‘My Courses’ page.
            </p>
        </div>
        <p>
            We offer HIV, sexually transmitted infection (STI), and viral hepatitis trainings across New York State.
        </p>
        <p>
            The trainings on this site are intended for non-physician health and human services providers who offer HIV,
            STI, and viral hepatitis prevention, testing, care, and support services. All trainings are free of charge and
            funded by the New York State Department of Health AIDS Institute.
        </p>
        <p>
            For clinical trainings, please visit <a href="https://ceitraining.org" target="_blank">ceitraining.org</a>.
        </p>
        <p>
            If you require assistance with registering for a course, please contact the training center listed on the
            course description. For a list of training centers and their contact information, click
            <a href="#">here</a>.
        </p>
    </div>
</template>
<script>
    //import eventBus from "@/eventBus.js";
    import SideNav from "@/components/SideNav.vue";
    //import LoginComponent from "@/components/LoginComponent.vue";
    //import RegistrationModal from "@/components/RegistrationModal.vue";
    import imageUrl1 from "@/assets/img1.jpeg";
    import imageUrl2 from "@/assets/img2.jpeg";
    import imageUrl3 from "@/assets/img1.jpeg";
    import imageUrl4 from "@/assets/img4.jpeg";

    export default {
        name: "HomePage",
        components: {
            //LoginComponent,
            //RegistrationModal,
            SideNav,
        },
        data() {
            return {
                userName: localStorage.getItem("userName") || null,
                isUserLoggedIn: !!localStorage.getItem("jwtToken"),
                showLoginModal: false,
                showRegistrationForm: false,
                isSideNavRendered: false,
                currentImageIndex: 0,
                currentOffset: 0,
                images: [
                    { src: imageUrl1, alt: "Image 1" },
                    { src: imageUrl2, alt: "Image 2" },
                    { src: imageUrl3, alt: "Image 3" },
                    { src: imageUrl4, alt: "Image 4" },
                ],
            };
        },
        mounted() {
            this.startImageCarousel();
            this.isSideNavRendered = true;
        },
        beforeUnmount() {
            clearInterval(this.carouselInterval);
        },
        methods: {
    startImageCarousel() {
        this.carouselInterval = setInterval(() => {
            this.nextImage();
        }, 5000);
    },

            nextImage() {
                this.currentImageIndex = (this.currentImageIndex + 1) % this.images.length;
            },

            prevImage() {
                this.currentImageIndex =
                    (this.currentImageIndex - 1 + this.images.length) % this.images.length;
            },

            goToImage(i) {
                this.currentImageIndex = i;
            }
},
        computed: {
            prevIndex() {
                return (this.currentImageIndex - 1 + this.images.length) % this.images.length;
            },
            nextIndex() {
                return (this.currentImageIndex + 1) % this.images.length;
            }
        }
    };</script>
<style scoped>

    /* General Container */
    .home-page-container {
        font-family: Arial, sans-serif;
        line-height: 1.6;
        color: #333;
        padding: 20px;
        position: relative;
    }

    /* Header Section */
    .header-section {
        text-align: center;
        margin-bottom: 20px;
    }

        .header-section h1 {
            font-size: 1.8rem;
            color: #1e88e5;
        }

    .image-gallery {
        width: 650px;
        height: 500px;
        margin: 20px auto;
        position: relative;
        overflow: hidden;
    }

    .gallery-container {
        display: flex;
        transition: transform 0.5s ease;
    }

    .gallery-image {
        width: 650px;
        height: 500px;
        object-fit: cover;
        margin-right: 10px;
    }

    .arrow {
        background: rgba(0, 0, 0, 0.5);
        color: white;
        border: none;
        cursor: pointer;
        padding: 10px;
        border-radius: 50%;
        position: absolute;
        top: 50%;
        transform: translateY(-50%);
        z-index: 10;
        transition: background 0.3s;
    }

    .left-arrow {
        left: 10px;
    }

    .right-arrow {
        right: 10px;
    }

    .arrow:hover {
        background: rgba(0, 0, 0, 0.8);
    }

    .dot-container {
        text-align: center;
        margin-top: 10px;
    }

    .dot {
        height: 10px;
        width: 10px;
        margin: 0 5px;
        background-color: #bbb;
        border-radius: 50%;
        display: inline-block;
        cursor: pointer;
        transition: background 0.3s;
    }

        .dot.active {
            background-color: #717171;
        }

    /* Register Button */
    .register-btn {
        position: absolute;
        top: 20px;
        right: 20px;
        background-color: #4b0082;
        color: white;
        border: none;
        padding: 8px 12px;
        border-radius: 4px;
        cursor: pointer;
        transition: background-color 0.3s;
    }

        .register-btn:hover {
            background-color: #303f9f;
        }

    /* Modal Overlay */
    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    /* Modal */
    .modal {
        background-color: white;
        padding: 20px;
        border-radius: 8px;
        width: 300px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        text-align: center;
    }

        .modal h3 {
            margin-bottom: 20px;
            color: #3f51b5;
        }

        .modal .form-group {
            margin-bottom: 15px;
        }

        .modal input {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        .modal .btn-primary {
            background-color: #3f51b5;
            color: white;
            padding: 8px 12px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            width: 100%;
        }

            .modal .btn-primary:hover {
                background-color: #303f9f;
            }

        .modal .close-btn {
            margin-top: 10px;
            background: none;
            color: #3f51b5;
            border: none;
            cursor: pointer;
            font-size: 14px;
            text-decoration: underline;
        }

            .modal .close-btn:hover {
                color: #1e88e5;
            }
    /* =============== MODERN CAROUSEL =============== */

    .carousel-container {
        width: 100%;
        max-width: 1100px;
        margin: 40px auto;
        display: flex;
        align-items: center;
        justify-content: center;
        position: relative;
    }

    .carousel-track {
        width: 100%;
        display: flex;
        justify-content: center;
        align-items: center;
        overflow: hidden;
        position: relative;
    }

    /* Each image card */
    .carousel-card {
        width: 420px;
        height: 280px;
        margin: 0 12px;
        opacity: 0.5;
        transform: scale(0.8);
        transition: all 0.45s ease;
        border-radius: 18px;
        overflow: hidden;
        box-shadow: 0 10px 28px rgba(0,0,0,0.15);
    }

        /* CENTER IMAGE — highlighted */
        .carousel-card.active {
            opacity: 1;
            transform: scale(1);
            z-index: 5;
        }

        /* Image */
        .carousel-card img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

    /* Arrows */
    .nav-btn {
        width: 55px;
        height: 55px;
        border-radius: 50%;
        border: none;
        background: white;
        font-size: 28px;
        cursor: pointer;
        box-shadow: 0 6px 14px rgba(0,0,0,0.2);
        display: flex;
        align-items: center;
        justify-content: center;
        transition: 0.25s;
        z-index: 10;
    }

        .nav-btn:hover {
            background: #f0f0f0;
        }

        .nav-btn.left {
            position: absolute;
            left: -70px;
        }

        .nav-btn.right {
            position: absolute;
            right: -70px;
        }

    /* Dots */
    .dots {
        margin-top: 14px;
        text-align: center;
    }

    .dot {
        width: 10px;
        height: 10px;
        background: #ccc;
        border-radius: 50%;
        display: inline-block;
        margin: 0 5px;
        transition: 0.3s;
        cursor: pointer;
    }

        .dot.active {
            background: #333;
        }
    /* 🔥 MAIN SLIDER WRAPPER */
    .hero-carousel {
        width: 100%;
        max-width: 1100px;
        height: 380px;
        margin: 50px auto;
        display: flex;
        align-items: center;
        position: relative;
    }

    /* 🔥 TRACK HOLDS CARDS */
    .hero-track {
        width: 100%;
        height: 100%;
        position: relative;
        display: flex;
        justify-content: center;
        overflow: visible;
    }

    /* 🔥 SLIDE CARD */
    .hero-card {
        position: absolute;
        width: 500px;
        height: 320px;
        border-radius: 20px;
        overflow: hidden;
        opacity: 0;
        transform: scale(0.75) translateX(0);
        transition: all 0.55s ease;
        box-shadow: 0 12px 28px rgba(0,0,0,0.2);
    }

        /* MAIN IMAGE CENTER */
        .hero-card.active {
            opacity: 1;
            transform: scale(1) translateX(0);
            z-index: 5;
        }

        /* LEFT (previous image) */
        .hero-card.prev {
            opacity: 0.55;
            transform: scale(0.82) translateX(-340px);
            z-index: 3;
        }

        /* RIGHT (next image) */
        .hero-card.next {
            opacity: 0.55;
            transform: scale(0.82) translateX(340px);
            z-index: 3;
        }

        .hero-card img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

    /* 🔥 ARROWS */
    .hero-arrow {
        width: 55px;
        height: 55px;
        border-radius: 50%;
        border: none;
        background: white;
        box-shadow: 0 6px 18px rgba(0,0,0,0.25);
        font-size: 28px;
        cursor: pointer;
        position: absolute;
        top: 50%;
        transform: translateY(-50%);
        z-index: 10;
        transition: 0.25s;
    }

        .hero-arrow.left {
            left: -70px;
        }

        .hero-arrow.right {
            right: -70px;
        }

        .hero-arrow:hover {
            background: #f0f0f0;
        }

    /* 🔥 DOTS */
    .hero-dots {
        text-align: center;
        margin-top: 20px;
    }

    .hero-dot {
        width: 10px;
        height: 10px;
        background: #bbb;
        display: inline-block;
        margin: 0 6px;
        border-radius: 50%;
        cursor: pointer;
        transition: 0.3s;
    }

        .hero-dot.active {
            background: #333;
        }
</style>