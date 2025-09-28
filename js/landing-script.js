// Analytics and Tracking System
class GameAnalytics {
    constructor() {
        this.sessionId = this.generateSessionId();
        this.userId = this.getOrCreateUserId();
        this.startTime = Date.now();
        this.pageViews = 0;
        this.events = [];
        this.init();
    }

    init() {
        this.trackPageView();
        this.setupEventListeners();
        this.setupScrollTracking();
        this.setupTimeTracking();
        this.setupFormTracking();
    }

    generateSessionId() {
        return 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    getOrCreateUserId() {
        let userId = localStorage.getItem('game_user_id');
        if (!userId) {
            userId = 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
            localStorage.setItem('game_user_id', userId);
        }
        return userId;
    }

    trackPageView() {
        this.pageViews++;
        const pageData = {
            event: 'page_view',
            page: window.location.pathname,
            title: document.title,
            url: window.location.href,
            referrer: document.referrer,
            timestamp: new Date().toISOString(),
            sessionId: this.sessionId,
            userId: this.userId,
            userAgent: navigator.userAgent,
            screenResolution: `${screen.width}x${screen.height}`,
            viewportSize: `${window.innerWidth}x${window.innerHeight}`,
            language: navigator.language,
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
        };
        
        this.sendEvent(pageData);
        console.log('Page view tracked:', pageData);
    }

    trackEvent(eventName, category = 'general', properties = {}) {
        const eventData = {
            event: eventName,
            category: category,
            properties: properties,
            timestamp: new Date().toISOString(),
            sessionId: this.sessionId,
            userId: this.userId,
            page: window.location.pathname,
            timeOnPage: Date.now() - this.startTime
        };

        this.events.push(eventData);
        this.sendEvent(eventData);
        console.log('Event tracked:', eventData);
    }

    sendEvent(eventData) {
        // Send to Google Analytics
        if (typeof gtag !== 'undefined') {
            gtag('event', eventData.event, {
                event_category: eventData.category,
                event_label: eventData.properties?.label || '',
                value: eventData.properties?.value || 0,
                custom_parameters: eventData.properties
            });
        }

        // Send to custom analytics endpoint (you can replace this with your own API)
        this.sendToCustomEndpoint(eventData);
    }

    async sendToCustomEndpoint(eventData) {
        // Temporarily disable analytics to avoid spam
        return; // Skip sending to avoid 405 errors
        
        try {
            // Replace with your actual analytics endpoint
            const response = await fetch('/api/analytics', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(eventData)
            });
            
            if (!response.ok) {
                console.warn('Analytics endpoint not available');
            }
        } catch (error) {
            console.warn('Failed to send analytics data:', error);
        }
    }

    setupEventListeners() {
        // Track button clicks
        document.addEventListener('click', (e) => {
            if (e.target.matches('button, .btn, a[href^="#"]')) {
                const element = e.target;
                const text = element.textContent.trim();
                const href = element.getAttribute('href');
                
                this.trackEvent('button_click', 'interaction', {
                    element: element.tagName.toLowerCase(),
                    text: text,
                    href: href,
                    className: element.className,
                    id: element.id
                });
            }
        });

        // Track external link clicks
        document.addEventListener('click', (e) => {
            if (e.target.matches('a[href^="http"]')) {
                this.trackEvent('external_link_click', 'navigation', {
                    url: e.target.href,
                    text: e.target.textContent.trim()
                });
            }
        });

        // Track image clicks
        document.addEventListener('click', (e) => {
            if (e.target.matches('img')) {
                this.trackEvent('image_click', 'interaction', {
                    src: e.target.src,
                    alt: e.target.alt,
                    className: e.target.className
                });
            }
        });
    }

    setupScrollTracking() {
        let scrollPercentages = [25, 50, 75, 100];
        let trackedPercentages = new Set();

        const trackScroll = () => {
            const scrollTop = window.pageYOffset;
            const docHeight = document.documentElement.scrollHeight - window.innerHeight;
            const scrollPercent = Math.round((scrollTop / docHeight) * 100);

            scrollPercentages.forEach(percentage => {
                if (scrollPercent >= percentage && !trackedPercentages.has(percentage)) {
                    trackedPercentages.add(percentage);
                    this.trackEvent('scroll_depth', 'engagement', {
                        percentage: percentage,
                        scrollTop: scrollTop,
                        docHeight: docHeight
                    });
                }
            });
        };

        window.addEventListener('scroll', this.throttle(trackScroll, 1000));
    }

    setupTimeTracking() {
        // Track time on page
        setInterval(() => {
            const timeOnPage = Date.now() - this.startTime;
            if (timeOnPage % 30000 === 0) { // Every 30 seconds
                this.trackEvent('time_on_page', 'engagement', {
                    seconds: Math.round(timeOnPage / 1000)
                });
            }
        }, 1000);

        // Track when user leaves page
        window.addEventListener('beforeunload', () => {
            const timeOnPage = Date.now() - this.startTime;
            this.trackEvent('page_exit', 'engagement', {
                timeOnPage: Math.round(timeOnPage / 1000),
                totalEvents: this.events.length
            });
        });
    }

    setupFormTracking() {
        // Track form interactions
        document.addEventListener('submit', (e) => {
            const form = e.target;
            this.trackEvent('form_submit', 'conversion', {
                formId: form.id,
                formClass: form.className,
                action: form.action,
                method: form.method
            });
        });

        // Track form field focus
        document.addEventListener('focus', (e) => {
            if (e.target.matches('input, textarea, select')) {
                this.trackEvent('form_field_focus', 'interaction', {
                    fieldType: e.target.type,
                    fieldName: e.target.name,
                    fieldId: e.target.id
                });
            }
        }, true);
    }

    // Utility function to throttle events
    throttle(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Get analytics summary
    getAnalyticsSummary() {
        return {
            sessionId: this.sessionId,
            userId: this.userId,
            pageViews: this.pageViews,
            totalEvents: this.events.length,
            timeOnPage: Date.now() - this.startTime,
            events: this.events
        };
    }
}

// Navigation functionality
class Navigation {
    constructor() {
        this.navbar = document.querySelector('.navbar');
        this.hamburger = document.querySelector('.hamburger');
        this.navMenu = document.querySelector('.nav-menu');
        this.navLinks = document.querySelectorAll('.nav-link');
        this.init();
    }

    init() {
        this.setupScrollEffect();
        this.setupMobileMenu();
        this.setupSmoothScrolling();
    }

    setupScrollEffect() {
        let lastScrollTop = 0;
        
        window.addEventListener('scroll', () => {
            const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            
            if (scrollTop > lastScrollTop && scrollTop > 100) {
                // Scrolling down
                this.navbar.style.transform = 'translateY(-100%)';
            } else {
                // Scrolling up
                this.navbar.style.transform = 'translateY(0)';
            }
            
            lastScrollTop = scrollTop;
        });
    }

    setupMobileMenu() {
        this.hamburger.addEventListener('click', () => {
            this.hamburger.classList.toggle('active');
            this.navMenu.classList.toggle('active');
        });

        // Close mobile menu when clicking on a link
        this.navLinks.forEach(link => {
            link.addEventListener('click', () => {
                this.hamburger.classList.remove('active');
                this.navMenu.classList.remove('active');
            });
        });
    }

    setupSmoothScrolling() {
        this.navLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                const href = link.getAttribute('href');
                if (href.startsWith('#')) {
                    e.preventDefault();
                    const targetId = href.substring(1);
                    const targetElement = document.getElementById(targetId);
                    
                    if (targetElement) {
                        const offsetTop = targetElement.offsetTop - 70; // Account for fixed navbar
                        window.scrollTo({
                            top: offsetTop,
                            behavior: 'smooth'
                        });
                    }
                }
            });
        });
    }
}

// Newsletter functionality
class Newsletter {
    constructor() {
        this.modal = document.getElementById('newsletterModal');
        this.form = document.getElementById('newsletterForm');
        this.closeBtn = document.querySelector('.close');
        this.init();
    }

    init() {
        this.setupEventListeners();
    }

    setupEventListeners() {
        // Open modal when newsletter button is clicked
        document.addEventListener('click', (e) => {
            if (e.target.textContent && e.target.textContent.includes('Đăng Ký Nhận Tin')) {
                e.preventDefault();
                this.openModal();
            }
        });

        // Close modal - check if element exists
        if (this.closeBtn) {
            this.closeBtn.addEventListener('click', () => {
                this.closeModal();
            });
        }

        // Close modal when clicking outside
        if (this.modal) {
            window.addEventListener('click', (e) => {
                if (e.target === this.modal) {
                    this.closeModal();
                }
            });
        }

        // Handle form submission - check if form exists
        if (this.form) {
            this.form.addEventListener('submit', (e) => {
                e.preventDefault();
                this.handleSubmit();
            });
        }
    }

    openModal() {
        this.modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
        analytics.trackEvent('newsletter_modal_open', 'conversion');
    }

    closeModal() {
        this.modal.style.display = 'none';
        document.body.style.overflow = 'auto';
        analytics.trackEvent('newsletter_modal_close', 'conversion');
    }

    async handleSubmit() {
        const email = this.form.querySelector('input[type="email"]').value;
        
        analytics.trackEvent('newsletter_signup_attempt', 'conversion', {
            email: email
        });

        try {
            // Simulate API call (replace with actual endpoint)
            await new Promise(resolve => setTimeout(resolve, 1000));
            
            this.showSuccessMessage();
            analytics.trackEvent('newsletter_signup_success', 'conversion', {
                email: email
            });
        } catch (error) {
            this.showErrorMessage();
            analytics.trackEvent('newsletter_signup_error', 'conversion', {
                error: error.message
            });
        }
    }

    showSuccessMessage() {
        const form = this.form;
        form.innerHTML = `
            <div style="text-align: center; padding: 20px;">
                <i class="fas fa-check-circle" style="font-size: 3rem; color: #4ade80; margin-bottom: 15px;"></i>
                <h4 style="color: #2d3748; margin-bottom: 10px;">Đăng ký thành công!</h4>
                <p style="color: #718096;">Cảm ơn bạn đã đăng ký nhận tin. Chúng tôi sẽ gửi thông báo về cập nhật game sớm nhất.</p>
            </div>
        `;
        
        setTimeout(() => {
            this.closeModal();
            this.resetForm();
        }, 3000);
    }

    showErrorMessage() {
        const form = this.form;
        form.innerHTML = `
            <div style="text-align: center; padding: 20px;">
                <i class="fas fa-exclamation-triangle" style="font-size: 3rem; color: #e53e3e; margin-bottom: 15px;"></i>
                <h4 style="color: #2d3748; margin-bottom: 10px;">Có lỗi xảy ra!</h4>
                <p style="color: #718096;">Vui lòng thử lại sau.</p>
                <button onclick="newsletter.resetForm()" style="margin-top: 15px; padding: 10px 20px; background: #e53e3e; color: white; border: none; border-radius: 5px; cursor: pointer;">Thử lại</button>
            </div>
        `;
    }

    resetForm() {
        this.form.innerHTML = `
            <input type="email" placeholder="Nhập email của bạn" required>
            <button type="submit">Đăng Ký</button>
        `;
        
        // Re-attach event listener
        this.form.addEventListener('submit', (e) => {
            e.preventDefault();
            this.handleSubmit();
        });
    }
}

// Story Carousel
class StoryCarousel {
    constructor() {
        this.currentSlide = 0;
        this.slides = document.querySelectorAll('.story-slide');
        this.totalSlides = this.slides.length;
        this.autoPlayInterval = null;
        this.isAutoPlaying = false;
        this.init();
    }

    init() {
        this.createIndicators();
        this.setupEventListeners();
        this.updateSlide();
        this.startAutoPlay();
    }

    createIndicators() {
        const indicatorsContainer = document.getElementById('storyIndicators');
        indicatorsContainer.innerHTML = '';

        for (let i = 0; i < this.totalSlides; i++) {
            const indicator = document.createElement('div');
            indicator.className = 'story-indicator';
            if (i === 0) indicator.classList.add('active');
            indicator.addEventListener('click', () => this.goToSlide(i));
            indicatorsContainer.appendChild(indicator);
        }
    }

    setupEventListeners() {
        // Navigation buttons
        document.getElementById('prevBtn').addEventListener('click', () => {
            this.prevSlide();
            this.trackEvent('story_navigation', 'prev');
        });

        document.getElementById('nextBtn').addEventListener('click', () => {
            this.nextSlide();
            this.trackEvent('story_navigation', 'next');
        });

        // Auto-play toggle
        document.getElementById('autoPlayToggle').addEventListener('click', () => {
            this.toggleAutoPlay();
        });

        // Keyboard navigation
        document.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowLeft') {
                this.prevSlide();
            } else if (e.key === 'ArrowRight') {
                this.nextSlide();
            }
        });

        // Touch/swipe support
        this.setupTouchEvents();
    }

    setupTouchEvents() {
        let startX = 0;
        let endX = 0;

        const carousel = document.querySelector('.story-carousel');

        carousel.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
        });

        carousel.addEventListener('touchend', (e) => {
            endX = e.changedTouches[0].clientX;
            this.handleSwipe(startX, endX);
        });
    }

    handleSwipe(startX, endX) {
        const threshold = 50;
        const diff = startX - endX;

        if (Math.abs(diff) > threshold) {
            if (diff > 0) {
                this.nextSlide();
            } else {
                this.prevSlide();
            }
        }
    }

    goToSlide(index) {
        this.currentSlide = index;
        this.updateSlide();
        this.trackEvent('story_navigation', 'indicator', { slide: index });
    }

    nextSlide() {
        this.currentSlide = (this.currentSlide + 1) % this.totalSlides;
        this.updateSlide();
    }

    prevSlide() {
        this.currentSlide = (this.currentSlide - 1 + this.totalSlides) % this.totalSlides;
        this.updateSlide();
    }

    updateSlide() {
        const slidesContainer = document.getElementById('storySlides');
        const progressBar = document.getElementById('storyProgress');
        const indicators = document.querySelectorAll('.story-indicator');

        // Update slide position
        slidesContainer.style.transform = `translateX(-${this.currentSlide * 100}%)`;

        // Update progress bar
        const progress = ((this.currentSlide + 1) / this.totalSlides) * 100;
        progressBar.style.width = `${progress}%`;

        // Update indicators
        indicators.forEach((indicator, index) => {
            indicator.classList.toggle('active', index === this.currentSlide);
        });

        // Track slide view
        this.trackEvent('story_slide_view', 'engagement', {
            slide: this.currentSlide,
            chapter: this.currentSlide + 1
        });
    }

    startAutoPlay() {
        this.isAutoPlaying = true;
        this.updateAutoPlayButton();
        
        this.autoPlayInterval = setInterval(() => {
            this.nextSlide();
        }, 5000); // Change slide every 5 seconds
    }

    stopAutoPlay() {
        this.isAutoPlaying = false;
        this.updateAutoPlayButton();
        
        if (this.autoPlayInterval) {
            clearInterval(this.autoPlayInterval);
            this.autoPlayInterval = null;
        }
    }

    toggleAutoPlay() {
        if (this.isAutoPlaying) {
            this.stopAutoPlay();
        } else {
            this.startAutoPlay();
        }
        
        this.trackEvent('story_autoplay_toggle', 'interaction', {
            autoplay: this.isAutoPlaying
        });
    }

    updateAutoPlayButton() {
        const button = document.getElementById('autoPlayToggle');
        const icon = button.querySelector('i');
        
        if (this.isAutoPlaying) {
            button.classList.add('active');
            icon.className = 'fas fa-pause';
            button.innerHTML = '<i class="fas fa-pause"></i> Tạm dừng';
        } else {
            button.classList.remove('active');
            icon.className = 'fas fa-play';
            button.innerHTML = '<i class="fas fa-play"></i> Tự động phát';
        }
    }

    trackEvent(eventName, category, properties = {}) {
        if (typeof trackEvent === 'function') {
            trackEvent(eventName, category, {
                ...properties,
                currentSlide: this.currentSlide,
                totalSlides: this.totalSlides
            });
        }
    }

    // Pause auto-play when user interacts
    pauseOnInteraction() {
        this.stopAutoPlay();
        // Restart auto-play after 10 seconds of inactivity
        setTimeout(() => {
            if (!this.isAutoPlaying) {
                this.startAutoPlay();
            }
        }, 10000);
    }
}

// Animation utilities
class Animations {
    constructor() {
        this.init();
    }

    init() {
        this.setupIntersectionObserver();
        this.setupParallaxEffect();
    }

    setupIntersectionObserver() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                    analytics.trackEvent('section_view', 'engagement', {
                        section: entry.target.id || entry.target.className
                    });
                }
            });
        }, observerOptions);

        // Observe sections
        document.querySelectorAll('section').forEach(section => {
            observer.observe(section);
        });

        // Observe feature cards
        document.querySelectorAll('.feature-card').forEach(card => {
            observer.observe(card);
        });
    }

    setupParallaxEffect() {
        window.addEventListener('scroll', () => {
            const scrolled = window.pageYOffset;
            const parallaxElements = document.querySelectorAll('.floating-flower');
            
            parallaxElements.forEach((element, index) => {
                const speed = 0.5 + (index * 0.1);
                const yPos = -(scrolled * speed);
                element.style.transform = `translateY(${yPos}px)`;
            });
        });
    }
}

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    console.log('🚀 DOM loaded, khởi tạo các components...');
    
    // Initialize analytics
    window.analytics = new GameAnalytics();
    
    // Initialize navigation
    window.navigation = new Navigation();
    
    // Initialize newsletter
    window.newsletter = new Newsletter();
    
    // Initialize story carousel
    window.storyCarousel = new StoryCarousel();
    
    // Initialize advanced hero carousel với delay nhẹ để đảm bảo DOM sẵn sàng
    setTimeout(() => {
        console.log('🎠 Khởi tạo Advanced Hero Carousel...');
        window.advancedHeroCarousel = new AdvancedHeroCarousel();
    }, 100);
    
    // Timeline removed - no longer needed
    
    // Initialize gameplay guide
    window.gameplayGuide = new GameplayGuide();
    
    // Initialize payment system
    window.paymentSystem = new PaymentSystem();
    
    // Initialize animations
    window.animations = new Animations();
    
    // Track page load time
    window.addEventListener('load', () => {
        const loadTime = Date.now() - analytics.startTime;
        analytics.trackEvent('page_load', 'performance', {
            loadTime: loadTime
        });
    });
    
    console.log('Vườn Rực Rỡ website initialized successfully!');
});

// Gameplay Guide System
class GameplayGuide {
    constructor() {
        this.init();
    }

    init() {
        this.setupTabs();
        this.trackTabInteractions();
    }

    setupTabs() {
        const tabButtons = document.querySelectorAll('.tab-btn');
        const tabPanels = document.querySelectorAll('.tab-panel');

        tabButtons.forEach(button => {
            button.addEventListener('click', () => {
                const targetTab = button.getAttribute('data-tab');
                
                // Remove active class from all buttons and panels
                tabButtons.forEach(btn => btn.classList.remove('active'));
                tabPanels.forEach(panel => panel.classList.remove('active'));
                
                // Add active class to clicked button and corresponding panel
                button.classList.add('active');
                document.getElementById(targetTab).classList.add('active');
                
                // Track tab switch
                this.trackEvent('tab_switch', 'gameplay_guide', {
                    tab: targetTab
                });
            });
        });
    }

    trackTabInteractions() {
        // Track when users view different sections
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const tabId = entry.target.id;
                    this.trackEvent('tab_view', 'gameplay_guide', {
                        tab: tabId,
                        timestamp: Date.now()
                    });
                }
            });
        }, { threshold: 0.5 });

        // Observe all tab panels
        document.querySelectorAll('.tab-panel').forEach(panel => {
            observer.observe(panel);
        });
    }

    trackEvent(eventName, category, properties = {}) {
        if (typeof trackEvent === 'function') {
            trackEvent(eventName, category, properties);
        }
    }
}

// Payment System
class PaymentSystem {
    constructor() {
        this.init();
    }

    init() {
        this.setupPaymentButtons();
        this.createPaymentModal();
    }

    setupPaymentButtons() {
        document.addEventListener('click', (e) => {
            if (e.target.closest('.payment-btn')) {
                const card = e.target.closest('.payment-card');
                const packageType = card.classList.contains('featured') ? 'premium' : 
                                  card.querySelector('.payment-badge').textContent.toLowerCase();
                this.openPaymentModal(packageType);
            }
        });
    }

    createPaymentModal() {
        const modalHTML = `
            <div id="paymentModal" class="modal">
                <div class="modal-content payment-modal">
                    <span class="close">&times;</span>
                    <h3 id="paymentTitle">Nạp Tiền</h3>
                    <div class="payment-package-info">
                        <div class="package-icon">
                            <i id="packageIcon" class="fas fa-coins"></i>
                        </div>
                        <div class="package-details">
                            <h4 id="packageName">Gói Cơ Bản</h4>
                            <div class="package-price">
                                <span id="packagePrice">50,000</span>
                                <span class="currency">VNĐ</span>
                            </div>
                        </div>
                    </div>
                    <div class="payment-form">
                        <div class="form-group">
                            <label>Chọn phương thức thanh toán:</label>
                            <div class="payment-methods-modal">
                                <label class="method-option">
                                    <input type="radio" name="paymentMethod" value="momo" checked>
                                    <img src="images/payment/momo.png" alt="MoMo">
                                    <span>MoMo</span>
                                </label>
                                <label class="method-option">
                                    <input type="radio" name="paymentMethod" value="zalopay">
                                    <img src="images/payment/zalopay.png" alt="ZaloPay">
                                    <span>ZaloPay</span>
                                </label>
                                <label class="method-option">
                                    <input type="radio" name="paymentMethod" value="vnpay">
                                    <img src="images/payment/vnpay.png" alt="VNPay">
                                    <span>VNPay</span>
                                </label>
                                <label class="method-option">
                                    <input type="radio" name="paymentMethod" value="banking">
                                    <img src="images/payment/banking.png" alt="Banking">
                                    <span>Banking</span>
                                </label>
                            </div>
                        </div>
                        <div class="form-group">
                            <label for="phoneNumber">Số điện thoại:</label>
                            <input type="tel" id="phoneNumber" placeholder="Nhập số điện thoại" required>
                        </div>
                        <button type="submit" class="btn btn-primary payment-submit">
                            <i class="fas fa-credit-card"></i>
                            Thanh Toán
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHTML);
        this.setupModalEvents();
    }

    setupModalEvents() {
        const modal = document.getElementById('paymentModal');
        const closeBtn = modal.querySelector('.close');
        const submitBtn = modal.querySelector('.payment-submit');

        closeBtn.addEventListener('click', () => {
            this.closePaymentModal();
        });

        window.addEventListener('click', (e) => {
            if (e.target === modal) {
                this.closePaymentModal();
            }
        });

        submitBtn.addEventListener('click', (e) => {
            e.preventDefault();
            this.processPayment();
        });
    }

    openPaymentModal(packageType) {
        const modal = document.getElementById('paymentModal');
        const packages = {
            'phổ biến': {
                name: 'Gói Cơ Bản',
                price: '50,000',
                icon: 'fas fa-coins'
            },
            'khuyến mãi': {
                name: 'Gói Cao Cấp',
                price: '150,000',
                icon: 'fas fa-gem'
            },
            'vip': {
                name: 'Gói Tối Thượng',
                price: '500,000',
                icon: 'fas fa-crown'
            }
        };

        const pkg = packages[packageType] || packages['phổ biến'];
        
        document.getElementById('packageName').textContent = pkg.name;
        document.getElementById('packagePrice').textContent = pkg.price;
        document.getElementById('packageIcon').className = pkg.icon;
        
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';

        trackEvent('payment_modal_open', 'conversion', {
            package: packageType,
            price: pkg.price
        });
    }

    closePaymentModal() {
        const modal = document.getElementById('paymentModal');
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
        
        trackEvent('payment_modal_close', 'conversion');
    }

    async processPayment() {
        const phoneNumber = document.getElementById('phoneNumber').value;
        const paymentMethod = document.querySelector('input[name="paymentMethod"]:checked').value;
        
        if (!phoneNumber) {
            alert('Vui lòng nhập số điện thoại!');
            return;
        }

        trackEvent('payment_attempt', 'conversion', {
            phone: phoneNumber,
            method: paymentMethod
        });

        // Simulate payment processing
        const submitBtn = document.querySelector('.payment-submit');
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
        submitBtn.disabled = true;

        try {
            // Simulate API call
            await new Promise(resolve => setTimeout(resolve, 2000));
            
            this.showPaymentSuccess();
            trackEvent('payment_success', 'conversion', {
                phone: phoneNumber,
                method: paymentMethod
            });
        } catch (error) {
            this.showPaymentError();
            trackEvent('payment_error', 'conversion', {
                error: error.message
            });
        } finally {
            submitBtn.innerHTML = originalText;
            submitBtn.disabled = false;
        }
    }

    showPaymentSuccess() {
        const modal = document.getElementById('paymentModal');
        modal.innerHTML = `
            <div class="modal-content payment-modal">
                <div style="text-align: center; padding: 40px;">
                    <i class="fas fa-check-circle" style="font-size: 4rem; color: #38a169; margin-bottom: 20px;"></i>
                    <h3 style="color: #2d3748; margin-bottom: 15px;">Thanh toán thành công!</h3>
                    <p style="color: #718096; margin-bottom: 20px;">Cảm ơn bạn đã nạp tiền. Vật phẩm sẽ được gửi vào tài khoản game của bạn.</p>
                    <button class="btn btn-primary" onclick="location.reload()">
                        <i class="fas fa-home"></i>
                        Về trang chủ
                    </button>
                </div>
            </div>
        `;
    }

    showPaymentError() {
        const modal = document.getElementById('paymentModal');
        modal.innerHTML = `
            <div class="modal-content payment-modal">
                <div style="text-align: center; padding: 40px;">
                    <i class="fas fa-exclamation-triangle" style="font-size: 4rem; color: #e53e3e; margin-bottom: 20px;"></i>
                    <h3 style="color: #2d3748; margin-bottom: 15px;">Thanh toán thất bại!</h3>
                    <p style="color: #718096; margin-bottom: 20px;">Có lỗi xảy ra trong quá trình thanh toán. Vui lòng thử lại.</p>
                    <button class="btn btn-primary" onclick="location.reload()">
                        <i class="fas fa-redo"></i>
                        Thử lại
                    </button>
                </div>
            </div>
        `;
    }
}

// Advanced Hero Carousel
class AdvancedHeroCarousel {
    constructor() {
        this.currentIndex = 0;
        this.items = document.querySelectorAll('.carousel-item');
        this.dots = document.querySelectorAll('.dot');
        this.progressBar = document.getElementById('progressBar');
        this.totalItems = this.items.length;
        this.autoPlayInterval = null;
        this.isAutoPlaying = false; // Tắt autoplay
        
        console.log('🔧 AdvancedHeroCarousel khởi tạo:', {
            totalItems: this.totalItems,
            items: this.items.length,
            dots: this.dots.length,
            progressBar: this.progressBar
        });
        
        if (this.totalItems > 0) {
            this.init();
        } else {
            console.error('❌ Không tìm thấy carousel items!');
        }
    }

    init() {
        console.log('🔧 Khởi tạo carousel...');
        this.setupEventListeners();
        this.updateCarousel();
        // this.startAutoPlay(); // Tắt autoplay
        console.log('✅ Carousel đã sẵn sàng!');
    }

    setupEventListeners() {
        // Navigation buttons
        const prevBtn = document.getElementById('prevBtn');
        const nextBtn = document.getElementById('nextBtn');
        
        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                this.prevSlide();
                this.resetAutoPlay();
            });
        }
        
        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                this.nextSlide();
                this.resetAutoPlay();
            });
        }

        // Dot indicators
        this.dots.forEach((dot, index) => {
            dot.addEventListener('click', () => {
                this.goToSlide(index);
                this.resetAutoPlay();
            });
        });

        // Item click
        this.items.forEach((item, index) => {
            item.addEventListener('click', () => {
                if (index !== this.currentIndex) {
                    this.goToSlide(index);
                    this.resetAutoPlay();
                }
            });
        });

        // Pause on hover
        const carousel = document.querySelector('.advanced-carousel');
        if (carousel) {
            carousel.addEventListener('mouseenter', () => {
                this.pauseAutoPlay();
            });
            
            carousel.addEventListener('mouseleave', () => {
                this.resumeAutoPlay();
            });
        }

        // Keyboard navigation
        document.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowLeft') {
                this.prevSlide();
                this.resetAutoPlay();
            } else if (e.key === 'ArrowRight') {
                this.nextSlide();
                this.resetAutoPlay();
            }
        });

        // Touch support
        this.setupTouchEvents();
    }

    setupTouchEvents() {
        let startX = 0;
        let endX = 0;
        const carousel = document.querySelector('.advanced-carousel');
        
        if (carousel) {
            carousel.addEventListener('touchstart', (e) => {
                startX = e.touches[0].clientX;
            });
            
            carousel.addEventListener('touchend', (e) => {
                endX = e.changedTouches[0].clientX;
                this.handleSwipe(startX, endX);
            });
        }
    }

    handleSwipe(startX, endX) {
        const threshold = 50;
        const diff = startX - endX;
        
        if (Math.abs(diff) > threshold) {
            if (diff > 0) {
                this.nextSlide();
            } else {
                this.prevSlide();
            }
            this.resetAutoPlay();
        }
    }

    updateCarousel() {
        // Update items positioning
        this.items.forEach((item, index) => {
            item.classList.remove('active');
            
            // Calculate position relative to current active item
            const relativeIndex = (index - this.currentIndex + this.totalItems) % this.totalItems;
            
            // Remove all positioning classes
            item.style.left = '';
            item.style.transform = '';
            item.style.zIndex = '';
            
            if (index === this.currentIndex) {
                // Active item - center
                item.classList.add('active');
                item.style.left = '50%';
                item.style.transform = 'translateX(-50%) scale(1.35)';
                item.style.zIndex = '5';
            } else if (relativeIndex === 1) {
                // Next item - right side
                item.style.left = '90px';
                item.style.zIndex = '2';
            } else if (relativeIndex === 2) {
                // Far right
                item.style.left = '250px';
                item.style.zIndex = '1';
            } else if (relativeIndex === 3) {
                // Left side
                item.style.left = '-90px';
                item.style.zIndex = '2';
            }
        });

        // Update dots
        this.dots.forEach((dot, index) => {
            dot.classList.remove('active');
            if (index === this.currentIndex) {
                dot.classList.add('active');
            }
        });

        // Update progress bar
        const progress = ((this.currentIndex + 1) / this.totalItems) * 100;
        if (this.progressBar) {
            this.progressBar.style.width = `${progress}%`;
        }

        // Analytics (tạm tắt để tránh spam console)
        // if (window.analytics) {
        //     window.analytics.trackEvent('carousel_slide', 'interaction', {
        //         slide: this.currentIndex,
        //         total: this.totalItems
        //     });
        // }
    }

    nextSlide() {
        this.currentIndex = (this.currentIndex + 1) % this.totalItems;
        this.updateCarousel();
    }

    prevSlide() {
        this.currentIndex = (this.currentIndex - 1 + this.totalItems) % this.totalItems;
        this.updateCarousel();
    }

    goToSlide(index) {
        if (index >= 0 && index < this.totalItems) {
            this.currentIndex = index;
            this.updateCarousel();
        }
    }

    startAutoPlay() {
        if (this.isAutoPlaying) {
            console.log('🔄 Bắt đầu autoplay carousel...');
            this.autoPlayInterval = setInterval(() => {
                console.log('🔄 Auto-chuyển slide:', this.currentIndex, '->', (this.currentIndex + 1) % this.totalItems);
                this.nextSlide();
            }, 4000); // 4 giây mỗi slide để test nhanh hơn
        }
    }

    pauseAutoPlay() {
        if (this.autoPlayInterval) {
            clearInterval(this.autoPlayInterval);
        }
    }

    resumeAutoPlay() {
        if (this.isAutoPlaying) {
            this.startAutoPlay();
        }
    }

    resetAutoPlay() {
        this.pauseAutoPlay();
        this.resumeAutoPlay();
    }

    toggleAutoPlay() {
        this.isAutoPlaying = !this.isAutoPlaying;
        if (this.isAutoPlaying) {
            this.startAutoPlay();
        } else {
            this.pauseAutoPlay();
        }
    }
}

// Video Modal Functions
function openVideoModal() {
    const modal = document.getElementById('videoModal');
    const video = document.getElementById('trailerVideo');
    
    if (modal && video) {
        modal.style.display = 'block';
        document.body.style.overflow = 'hidden';
        
        // Small delay to ensure smooth animation
        setTimeout(() => {
            video.play().catch(e => {
                console.log('Video autoplay blocked:', e);
            });
        }, 300);
        
        // Track analytics
        if (window.analytics) {
            window.analytics.trackEvent('video_modal_open', 'engagement', {
                video: '0911.mp4',
                source: 'trailer_button'
            });
        }
    }
}

function closeVideoModal() {
    const modal = document.getElementById('videoModal');
    const video = document.getElementById('trailerVideo');
    
    if (modal && video) {
        modal.classList.add('closing');
        
        // Pause and reset video
        video.pause();
        video.currentTime = 0;
        
        setTimeout(() => {
            modal.style.display = 'none';
            modal.classList.remove('closing');
            document.body.style.overflow = 'auto';
        }, 300);
        
        // Track analytics
        if (window.analytics) {
            window.analytics.trackEvent('video_modal_close', 'engagement', {
                video: '0911.mp4',
                watchTime: video.currentTime
            });
        }
    }
}

// Keyboard support for video modal
document.addEventListener('keydown', (e) => {
    const modal = document.getElementById('videoModal');
    if (modal && modal.style.display === 'block') {
        if (e.key === 'Escape') {
            closeVideoModal();
        } else if (e.key === ' ') {
            e.preventDefault();
            const video = document.getElementById('trailerVideo');
            if (video) {
                if (video.paused) {
                    video.play();
                } else {
                    video.pause();
                }
            }
        }
    }
});

// Video event listeners for analytics
document.addEventListener('DOMContentLoaded', () => {
    const video = document.getElementById('trailerVideo');
    if (video) {
        video.addEventListener('play', () => {
            if (window.analytics) {
                window.analytics.trackEvent('video_play', 'engagement', {
                    video: '0911.mp4'
                });
            }
        });
        
        video.addEventListener('pause', () => {
            if (window.analytics) {
                window.analytics.trackEvent('video_pause', 'engagement', {
                    video: '0911.mp4',
                    currentTime: video.currentTime
                });
            }
        });
        
        video.addEventListener('ended', () => {
            if (window.analytics) {
                window.analytics.trackEvent('video_ended', 'engagement', {
                    video: '0911.mp4',
                    duration: video.duration
                });
            }
        });
    }
});

// Music Control Functions
let isMusicPlaying = false;
let musicStarted = false;

function toggleMusic() {
    const audio = document.getElementById('backgroundMusic');
    const musicBtn = document.getElementById('musicToggle');
    const musicIcon = document.getElementById('musicIcon');
    
    if (!audio) return;
    
    if (!musicStarted) {
        // First time playing - user interaction required
        audio.volume = 0.3; // Set volume to 30%
        musicStarted = true;
    }
    
    if (isMusicPlaying) {
        // Pause music
        audio.pause();
        musicBtn.classList.add('muted');
        musicIcon.className = 'fas fa-volume-mute';
        isMusicPlaying = false;
        
        // Track analytics
        if (window.analytics) {
            window.analytics.trackEvent('music_paused', 'audio', {
                track: 'Bóng Tối Tham Lam.mp3'
            });
        }
    } else {
        // Play music
        audio.play().then(() => {
            musicBtn.classList.remove('muted');
            musicIcon.className = 'fas fa-volume-up';
            isMusicPlaying = true;
            
            // Track analytics
            if (window.analytics) {
                window.analytics.trackEvent('music_played', 'audio', {
                    track: 'Bóng Tối Tham Lam.mp3'
                });
            }
        }).catch(e => {
            console.log('Audio playback failed:', e);
        });
    }
}

// Auto-start music when page loads (with user interaction)
document.addEventListener('DOMContentLoaded', () => {
    const audio = document.getElementById('backgroundMusic');
    if (audio) {
        // Set initial volume
        audio.volume = 0.3;
        
        // Add event listeners
        audio.addEventListener('ended', () => {
            isMusicPlaying = false;
            const musicBtn = document.getElementById('musicToggle');
            const musicIcon = document.getElementById('musicIcon');
            if (musicBtn && musicIcon) {
                musicBtn.classList.add('muted');
                musicIcon.className = 'fas fa-volume-mute';
            }
        });
        
        audio.addEventListener('error', (e) => {
            console.error('Audio loading error:', e);
        });
    }
});

// Try to auto-play music on first user interaction
let hasInteracted = false;
document.addEventListener('click', () => {
    if (!hasInteracted && !isMusicPlaying) {
        hasInteracted = true;
        const audio = document.getElementById('backgroundMusic');
        if (audio && !musicStarted) {
            // Auto-start music on first click
            setTimeout(() => {
                toggleMusic();
            }, 500);
        }
    }
});

// 3D Scroll Reveal System
class ScrollReveal3D {
    constructor() {
        this.elements = document.querySelectorAll('.scroll-reveal');
        this.windowHeight = window.innerHeight;
        this.init();
    }
    
    init() {
        this.bindEvents();
        this.checkElements();
    }
    
    bindEvents() {
        window.addEventListener('scroll', () => this.checkElements());
        window.addEventListener('resize', () => {
            this.windowHeight = window.innerHeight;
        });
    }
    
    checkElements() {
        this.elements.forEach(element => {
            if (this.isInViewport(element)) {
                this.revealElement(element);
            }
        });
    }
    
    isInViewport(element) {
        const rect = element.getBoundingClientRect();
        const threshold = this.windowHeight * 0.1; // 10% threshold
        
        return (
            rect.top >= -threshold &&
            rect.bottom <= (this.windowHeight + threshold)
        );
    }
    
    revealElement(element) {
        if (!element.classList.contains('revealed')) {
            element.classList.add('revealed');
            
            // Add animation class based on data attributes or existing classes
            if (element.classList.contains('slide-left') || 
                element.classList.contains('slide-right') || 
                element.classList.contains('slide-up') || 
                element.classList.contains('scale-in') || 
                element.classList.contains('flip-in')) {
                // Animation classes already applied
            }
            
            // Track analytics
            if (window.analytics) {
                window.analytics.trackEvent('scroll_reveal', 'engagement', {
                    element: element.className,
                    section: this.getSectionName(element)
                });
            }
        }
    }
    
    getSectionName(element) {
        const section = element.closest('section');
        return section ? section.id || 'unknown' : 'unknown';
    }
}

// Enhanced Scroll Effects for Story Section
class StoryScrollEffects {
    constructor() {
        this.storySection = document.getElementById('story');
        this.storySlides = document.querySelectorAll('.story-slide');
        this.init();
    }
    
    init() {
        if (!this.storySection) return;
        
        this.bindEvents();
    }
    
    bindEvents() {
        window.addEventListener('scroll', () => this.updateStoryEffects());
    }
    
    updateStoryEffects() {
        const rect = this.storySection.getBoundingClientRect();
        const windowHeight = window.innerHeight;
        const scrollProgress = 1 - (rect.top / windowHeight);
        
        if (scrollProgress > 0 && scrollProgress < 1.5) {
            this.storySlides.forEach((slide, index) => {
                const slideProgress = Math.max(0, scrollProgress - (index * 0.1));
                const transform = Math.min(slideProgress * 100, 100);
                
                if (slideProgress > 0) {
                    slide.style.transform = `
                        translateY(${Math.max(0, 50 - transform)}px) 
                        rotateX(${Math.max(0, 5 - slideProgress * 5)}deg)
                        scale(${Math.min(1, 0.9 + slideProgress * 0.1)})
                    `;
                    slide.style.opacity = Math.min(1, slideProgress * 2);
                }
            });
        }
    }
}

// Progressive Scroll Reveal System
class ProgressiveScrollReveal {
    constructor() {
        this.scrollProgress = document.getElementById('scrollProgress');
        this.elementsToReveal = document.querySelectorAll('.element-reveal');
        this.charElements = document.querySelectorAll('.char-reveal');
        this.textLines = document.querySelectorAll('.progressive-text .text-line');
        
        this.init();
    }
    
    init() {
        this.setupCharReveal();
        this.bindEvents();
        this.updateScrollProgress();
        this.checkElements();
    }
    
    setupCharReveal() {
        this.charElements.forEach(element => {
            const text = element.getAttribute('data-text') || element.textContent;
            element.innerHTML = '';
            
            [...text].forEach((char, index) => {
                const span = document.createElement('span');
                span.className = 'char';
                span.textContent = char === ' ' ? '\u00A0' : char;
                span.style.transitionDelay = `${index * 0.05}s`;
                element.appendChild(span);
            });
        });
    }
    
    bindEvents() {
        window.addEventListener('scroll', () => {
            this.updateScrollProgress();
            this.checkElements();
        });
        
        window.addEventListener('resize', () => {
            this.checkElements();
        });
    }
    
    updateScrollProgress() {
        const docHeight = document.documentElement.scrollHeight - window.innerHeight;
        const scrolled = window.pageYOffset;
        const progress = (scrolled / docHeight) * 100;
        
        if (this.scrollProgress) {
            this.scrollProgress.style.width = `${Math.min(progress, 100)}%`;
        }
    }
    
    checkElements() {
        // Check element reveals
        this.elementsToReveal.forEach(element => {
            if (this.isInViewport(element, 0.1)) {
                element.classList.add('in-view');
            }
        });
        
        // Check character reveals
        this.charElements.forEach(element => {
            if (this.isInViewport(element, 0.2)) {
                const chars = element.querySelectorAll('.char');
                chars.forEach(char => char.classList.add('revealed'));
            }
        });
        
        // Check text line reveals
        this.textLines.forEach(line => {
            if (this.isInViewport(line, 0.1)) {
                line.classList.add('revealed');
            }
        });
    }
    
    isInViewport(element, threshold = 0.1) {
        const rect = element.getBoundingClientRect();
        const windowHeight = window.innerHeight;
        const elementHeight = rect.height;
        
        // Element is considered "in view" when it's partially visible
        const elementTop = rect.top;
        const elementBottom = rect.bottom;
        
        // Calculate how much of the element is visible
        const visibleHeight = Math.min(elementBottom, windowHeight) - Math.max(elementTop, 0);
        const visibilityRatio = visibleHeight / elementHeight;
        
        return visibilityRatio > threshold;
    }
}

// Smooth Scroll-Triggered Animations
class SmoothScrollAnimations {
    constructor() {
        this.sections = document.querySelectorAll('section');
        this.init();
    }
    
    init() {
        this.bindEvents();
    }
    
    bindEvents() {
        window.addEventListener('scroll', () => this.updateAnimations());
    }
    
    updateAnimations() {
        const scrollY = window.pageYOffset;
        const windowHeight = window.innerHeight;
        
        this.sections.forEach(section => {
            const rect = section.getBoundingClientRect();
            const sectionTop = rect.top + scrollY;
            const sectionHeight = rect.height;
            
            // Calculate scroll progress within this section
            const sectionProgress = Math.max(0, Math.min(1, 
                (scrollY + windowHeight - sectionTop) / (sectionHeight + windowHeight)
            ));
            
            // Apply progressive animations based on scroll progress
            this.animateSection(section, sectionProgress);
        });
    }
    
    animateSection(section, progress) {
        if (section.id === 'story') {
            const slides = section.querySelectorAll('.story-slide');
            
            slides.forEach((slide, index) => {
                const slideProgress = Math.max(0, Math.min(1, 
                    (progress - (index * 0.15)) / 0.3
                ));
                
                if (slideProgress > 0) {
                    const opacity = Math.min(1, slideProgress * 2);
                    const translateY = Math.max(0, 50 * (1 - slideProgress));
                    const scale = 0.9 + (0.1 * slideProgress);
                    
                    slide.style.opacity = opacity;
                    slide.style.transform = `
                        translateY(${translateY}px) 
                        scale(${scale})
                    `;
                    
                    // Trigger text animations when slide becomes visible
                    if (slideProgress > 0.3) {
                        const textLines = slide.querySelectorAll('.text-line');
                        textLines.forEach((line, lineIndex) => {
                            setTimeout(() => {
                                line.classList.add('revealed');
                            }, lineIndex * 200);
                        });
                        
                        const chars = slide.querySelectorAll('.char');
                        chars.forEach(char => char.classList.add('revealed'));
                    }
                }
            });
        }
    }
}

// Initialize Scroll Systems
document.addEventListener('DOMContentLoaded', () => {
    // Initialize Progressive Scroll Reveal
    const progressiveReveal = new ProgressiveScrollReveal();
    
    // Initialize Smooth Scroll Animations
    const smoothAnimations = new SmoothScrollAnimations();
    
    // Initialize 3D Scroll Reveal
    const scrollReveal = new ScrollReveal3D();
    
    // Initialize Story Scroll Effects
    const storyEffects = new StoryScrollEffects();
    
    // Performance optimization - throttle scroll events
    let ticking = false;
    
    function updateScrollAnimations() {
        progressiveReveal.updateScrollProgress();
        progressiveReveal.checkElements();
        smoothAnimations.updateAnimations();
        scrollReveal.checkElements();
        storyEffects.updateStoryEffects();
        ticking = false;
    }
    
    window.addEventListener('scroll', () => {
        if (!ticking) {
            requestAnimationFrame(updateScrollAnimations);
            ticking = true;
        }
    });
});

// Global functions for tracking
function trackEvent(eventName, category = 'general', properties = {}) {
    if (window.analytics) {
        window.analytics.trackEvent(eventName, category, properties);
    }
}

// Backup carousel initialization function
function forceInitCarousel() {
    if (!window.advancedHeroCarousel) {
        console.log('🔧 Force initializing carousel...');
        setTimeout(() => {
            window.advancedHeroCarousel = new AdvancedHeroCarousel();
        }, 500);
    }
}

// Auto-retry carousel initialization after 3 seconds if not working
setTimeout(() => {
    if (!window.advancedHeroCarousel) {
        console.log('⚠️ Carousel chưa hoạt động, thử khởi tạo lại...');
        forceInitCarousel();
    } else {
        console.log('✅ Carousel đã hoạt động bình thường');
    }
}, 3000);

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { GameAnalytics, Navigation, Newsletter, Animations, HeroCarousel };
}
