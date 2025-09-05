// Portfolio filtering
const filterButtons = document.querySelectorAll('.filter-btn');
const portfolioItems = document.querySelectorAll('.portfolio-item');

filterButtons.forEach(button => {
    button.addEventListener('click', () => {
        // Update active button
        filterButtons.forEach(btn => btn.classList.remove('active'));
        button.classList.add('active');
        
        const filter = button.getAttribute('data-filter');
        
        portfolioItems.forEach(item => {
            if (filter === 'all' || item.getAttribute('data-category') === filter) {
                item.style.display = 'block';
                item.style.animation = 'fadeInUp 0.5s ease';
            } else {
                item.style.display = 'none';
            }
        });
    });
});

// Testimonials carousel
let currentTestimonial = 0;
const testimonialTrack = document.querySelector('.testimonial-track');
const testimonialCards = document.querySelectorAll('.testimonial-card');
const testimonialDots = document.querySelectorAll('.testimonial-dots .dot');
const testimonialPrev = document.querySelector('.testimonial-prev');
const testimonialNext = document.querySelector('.testimonial-next');

function updateTestimonial(index) {
    currentTestimonial = index;
    
    if (testimonialTrack) {
        testimonialTrack.style.transform = `translateX(-${index * 100}%)`;
    }
    
    // Update dots
    testimonialDots.forEach((dot, i) => {
        dot.classList.toggle('active', i === index);
    });
    
    // Update cards
    testimonialCards.forEach((card, i) => {
        card.classList.toggle('active', i === index);
    });
}

testimonialNext?.addEventListener('click', () => {
    currentTestimonial = (currentTestimonial + 1) % testimonialCards.length;
    updateTestimonial(currentTestimonial);
});

testimonialPrev?.addEventListener('click', () => {
    currentTestimonial = currentTestimonial === 0 ? testimonialCards.length - 1 : currentTestimonial - 1;
    updateTestimonial(currentTestimonial);
});

testimonialDots.forEach((dot, index) => {
    dot.addEventListener('click', () => {
        updateTestimonial(index);
    });
});

// Auto-rotate testimonials
setInterval(() => {
    if (testimonialCards.length > 0) {
        currentTestimonial = (currentTestimonial + 1) % testimonialCards.length;
        updateTestimonial(currentTestimonial);
    }
}, 5000);

// Pricing toggle
const pricingToggle = document.getElementById('pricingToggle');
const monthlyPrices = document.querySelectorAll('.monthly');
const yearlyPrices = document.querySelectorAll('.yearly');

pricingToggle?.addEventListener('change', () => {
    const isYearly = pricingToggle.checked;
    
    monthlyPrices.forEach(element => {
        element.classList.toggle('hidden', isYearly);
    });
    
    yearlyPrices.forEach(element => {
        element.classList.toggle('hidden', !isYearly);
    });
});

// FAQ accordion
const faqQuestions = document.querySelectorAll('.faq-question');

faqQuestions.forEach(question => {
    question.addEventListener('click', () => {
        const faqItem = question.parentElement;
        const answer = faqItem.querySelector('.faq-answer');
        const isActive = question.classList.contains('active');
        
        // Close all other FAQ items
        faqQuestions.forEach(q => {
            if (q !== question) {
                q.classList.remove('active');
                q.parentElement.querySelector('.faq-answer').classList.remove('active');
            }
        });
        
        // Toggle current item
        question.classList.toggle('active', !isActive);
        answer.classList.toggle('active', !isActive);
    });
});

// Newsletter form
const newsletterForm = document.getElementById('newsletterForm');
newsletterForm?.addEventListener('submit', (e) => {
    e.preventDefault();
    
    const emailInput = newsletterForm.querySelector('input[type="email"]');
    const email = emailInput.value;
    
    if (!isValidEmail(email)) {
        showNotification('Please enter a valid email address', 'error');
        return;
    }
    
    // Simulate subscription
    const submitButton = newsletterForm.querySelector('button[type="submit"]');
    const originalText = submitButton.innerHTML;
    
    submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Subscribing...';
    submitButton.disabled = true;
    
    setTimeout(() => {
        submitButton.innerHTML = originalText;
        submitButton.disabled = false;
        newsletterForm.reset();
        showNotification('Successfully subscribed to our newsletter! 🎉', 'success');
    }, 2000);
});

// Update navigation active state on scroll
function updateActiveNavLink() {
    const sections = document.querySelectorAll('section[id]');
    const navLinks = document.querySelectorAll('.nav-link');
    const scrollPosition = window.scrollY + 100;
    
    sections.forEach(section => {
        const sectionTop = section.offsetTop;
        const sectionHeight = section.offsetHeight;
        const sectionId = section.getAttribute('id');
        
        if (scrollPosition >= sectionTop && scrollPosition < sectionTop + sectionHeight) {
            navLinks.forEach(link => {
                link.classList.remove('active');
                if (link.getAttribute('href') === `#${sectionId}`) {
                    link.classList.add('active');
                }
            });
        }
    });
}

window.addEventListener('scroll', updateActiveNavLink);

// Enhanced scroll animations
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const scrollObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('animate-in');
            
            // Specific animations for different elements
            if (entry.target.classList.contains('portfolio-item')) {
                entry.target.style.animationDelay = `${Math.random() * 0.5}s`;
            }
            
            if (entry.target.classList.contains('team-member')) {
                entry.target.style.animationDelay = `${Math.random() * 0.3}s`;
            }
        }
    });
}, observerOptions);

// Observe elements for scroll animations
document.querySelectorAll('.portfolio-item, .team-member, .blog-card, .pricing-card').forEach(el => {
    scrollObserver.observe(el);
});

// Add CSS for scroll animations
const scrollAnimationStyles = `
    .portfolio-item,
    .team-member,
    .blog-card,
    .pricing-card {
        opacity: 0;
        transform: translateY(30px);
        transition: all 0.6s ease;
    }
    
    .portfolio-item.animate-in,
    .team-member.animate-in,
    .blog-card.animate-in,
    .pricing-card.animate-in {
        opacity: 1;
        transform: translateY(0);
    }
`;

const styleSheet = document.createElement('style');
styleSheet.textContent = scrollAnimationStyles;
document.head.appendChild(styleSheet);

// Lightbox functionality for portfolio
const portfolioBtns = document.querySelectorAll('[data-lightbox]');
portfolioBtns.forEach(btn => {
    btn.addEventListener('click', (e) => {
        e.preventDefault();
        const lightboxId = btn.getAttribute('data-lightbox');
        showLightbox(lightboxId);
    });
});

function showLightbox(id) {
    // Create lightbox overlay
    const lightbox = document.createElement('div');
    lightbox.className = 'lightbox-overlay';
    lightbox.innerHTML = `
        <div class="lightbox-content">
            <button class="lightbox-close">
                <i class="fas fa-times"></i>
            </button>
            <div class="lightbox-image">
                <div class="lightbox-placeholder">
                    <i class="fas fa-image"></i>
                    <h3>Project Preview</h3>
                    <p>This would show the full project details and images in a real implementation.</p>
                </div>
            </div>
        </div>
    `;
    
    // Add lightbox styles
    lightbox.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.9);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 10000;
        opacity: 0;
        transition: opacity 0.3s ease;
    `;
    
    const lightboxContent = lightbox.querySelector('.lightbox-content');
    lightboxContent.style.cssText = `
        position: relative;
        max-width: 90%;
        max-height: 90%;
        background: var(--bg-card);
        border-radius: 1rem;
        padding: 2rem;
        transform: scale(0.8);
        transition: transform 0.3s ease;
    `;
    
    const lightboxClose = lightbox.querySelector('.lightbox-close');
    lightboxClose.style.cssText = `
        position: absolute;
        top: 1rem;
        right: 1rem;
        background: var(--accent-primary);
        border: none;
        border-radius: 50%;
        width: 40px;
        height: 40px;
        color: white;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.2rem;
        transition: all 0.3s ease;
    `;
    
    const lightboxPlaceholder = lightbox.querySelector('.lightbox-placeholder');
    lightboxPlaceholder.style.cssText = `
        text-align: center;
        padding: 4rem 2rem;
        color: var(--text-primary);
    `;
    
    document.body.appendChild(lightbox);
    
    // Animate in
    setTimeout(() => {
        lightbox.style.opacity = '1';
        lightboxContent.style.transform = 'scale(1)';
    }, 10);
    
    // Close functionality
    lightboxClose.addEventListener('click', closeLightbox);
    lightbox.addEventListener('click', (e) => {
        if (e.target === lightbox) closeLightbox();
    });
    
    function closeLightbox() {
        lightbox.style.opacity = '0';
        lightboxContent.style.transform = 'scale(0.8)';
        setTimeout(() => {
            lightbox.remove();
        }, 300);
    }
    
    // Close on escape key
    const handleEscape = (e) => {
        if (e.key === 'Escape') {
            closeLightbox();
            document.removeEventListener('keydown', handleEscape);
        }
    };
    document.addEventListener('keydown', handleEscape);
}

// Pricing card interactions
document.querySelectorAll('.pricing-btn').forEach(btn => {
    btn.addEventListener('click', (e) => {
        e.preventDefault();
        const plan = btn.closest('.pricing-card').querySelector('h3').textContent;
        
        if (btn.textContent.includes('Contact Sales')) {
            showNotification(`Thanks for your interest in the ${plan} plan! Our sales team will contact you soon.`, 'info');
        } else {
            showNotification(`Thanks for choosing the ${plan} plan! Redirecting to checkout...`, 'success');
        }
    });
});

// Blog link interactions
document.querySelectorAll('.blog-link').forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        const articleTitle = link.closest('.blog-card').querySelector('h3').textContent;
        showNotification(`Opening article: "${articleTitle}"`, 'info');
    });
});

console.log('🎨 Enhanced features loaded successfully!');