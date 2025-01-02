<template>
    <div class="peer-certification-container">
        <!-- Apply Button -->
        <div class="apply-button-container">
            <button class="apply-btn">Apply for Peer Certification</button>
        </div>

        <h1>NYS Peer Worker Certification Information and Resources</h1>
        <p>
            The AIDS Institute (AI) has, for many years, recognized the important role
            that peer workers can play in improving health outcomes and many health and
            social service programs have a long history of successfully engaging clients
            in a wide range of activities. The NYS Peer Worker Certification Program is
            highlighted in the NYS Blueprint for Ending the AIDS Epidemic – peer-delivered
            services play a key role in meeting the state’s goals of increasing linkage
            and retention in care, rates of viral suppression, and preventing new infections.
        </p>

        <h3>Updated 11/27/2023</h3>
        <p>The New York State Peer Worker Certification Program is expanding!</p>
        <ul>
            <li>We are offering two new certification tracks:</li>
            <ul>
                <li><strong>PrEP Peer Worker</strong></li>
                <li><strong>Criminal Justice Peer Worker</strong></li>
            </ul>
            <li>
                Individuals must have the lived experience outlined below to be eligible for
                certification in these new tracks:
            </li>
            <ul>
                <li><strong>PrEP:</strong> lived experience of taking PrEP</li>
                <li>
                    <strong>Criminal Justice:</strong> incarceration in a federal or state prison,
                    county jail, parole, probation, or being charged with a felony.
                </li>
            </ul>
        </ul>

        <p>
            New courses for these certification tracks are under development. To become certified,
            an individual must complete a minimum of 68 hours of core training and 22 hours of specialized
            training. For details, see the
            <a href="#">PrEP Peer Worker Course Tracker</a> and the
            <a href="#">Criminal Justice Peer Worker Course Tracker</a>.
        </p>

        <p>
            If you have questions, please reach out to
            <a href="mailto:hivet&#64;health.ny.gov">hivet&#64;health.ny.gov</a>.
        </p>

        <!-- Resources Section -->
        <h2>Resources</h2>
        <ul>
            <li><a href="#">Peer Certification Frequently Asked Questions (FAQs)</a></li>
            <li><a href="#">Foundational Training</a></li>
            <li><a href="#">Core Competencies</a></li>
            <li>
                <a href="#">NYS Peer Worker Certification Program Code of Ethics</a>
                (also available in Spanish)
            </li>
            <li>
                <a href="#">Code of Ethics Violation Complaint Process and Forms</a>
            </li>
            <li><a href="#">Peer Worker Self-Care Worksheet</a></li>
            <li><a href="#">Process, Requirements, Application (For Peer Workers)</a></li>
            <li><a href="#">NYS Peer Worker Certification Program: Course Catalog</a></li>
            <li><a href="#">Course Tracker Forms</a></li>
            <li><a href="#">Suicide Prevention Course: Living Works START</a></li>
            <li><a href="#">Peer Certification Exam Study Guides</a></li>
            <li><a href="#">CEUs (Continuing Education Units) Guidance</a></li>
        </ul>

        <!-- Resources for Organizations -->
        <h2>Resources for Organizations and Supervisors</h2>
        <ul>
            <li><a href="#">Organizational Assessment for Peer-Delivered Services</a></li>
            <li><a href="#">Peer Worker Certification Program Supervisor Evaluation</a></li>
            <li><a href="#">Supervisors FAQ</a></li>
            <li><a href="#">Integrating Peer Workers into Health Care Services</a></li>
        </ul>

        <!-- Employment Section -->
        <h2>Employment & Benefits</h2>
        <ul>
            <li><a href="#">Pilot Program for PLWH Seeking Employment</a></li>
            <li><a href="#">Peer Worker Employment Opportunities LISTSERV</a></li>
            <li><a href="#">Empire State College Credits FAQ</a></li>
        </ul>

        <!-- Final Apply Button -->
        <div class="apply-button-container">
            <button class="apply-btn">Apply for Peer Certification</button>
        </div>
    </div>
</template>

<script>export default {
        name: "PeerCertificationPage",
    };</script>

<style scoped>
    .peer-certification-container {
        padding: 30px;
        max-width: 900px;
        margin: 0 auto;
        background-color: #f9f9f9;
        border-radius: 8px;
        box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
        font-family: Arial, sans-serif;
        color: #333;
    }

    .apply-button-container {
        display: flex;
        justify-content: center;
        margin: 20px 0;
    }

    .apply-btn {
        background-color: #3f51b5;
        color: white;
        border: none;
        padding: 12px 24px;
        font-size: 1rem;
        font-weight: bold;
        border-radius: 5px;
        cursor: pointer;
        transition: background-color 0.3s ease;
    }

        .apply-btn:hover {
            background-color: #2c387e;
        }

    h1, h2, h3 {
        color: #3f51b5;
    }

    h1 {
        font-size: 2rem;
        margin-bottom: 16px;
    }

    h2 {
        font-size: 1.6rem;
        margin-top: 24px;
    }

    h3 {
        font-size: 1.2rem;
    }

    p {
        margin: 16px 0;
    }

    ul {
        list-style-type: disc;
        margin: 16px 0;
        padding-left: 20px;
    }

        ul ul {
            list-style-type: circle;
            margin: 8px 0;
            padding-left: 20px;
        }

    a {
        color: #3f51b5;
        text-decoration: none;
    }

        a:hover {
            text-decoration: underline;
        }
</style>
