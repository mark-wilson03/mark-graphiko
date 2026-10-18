/**
 * MARK GRAPHIKO - Interactivity, Theme Toggling, Carousel & Gallery Engine
 */

// 1. THEME TOGGLER (Light & Dark Mode)
(function initTheme() {
    const savedTheme = localStorage.getItem('mark_graphiko_theme');
    const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    
    if (savedTheme === 'dark' || (!savedTheme && systemPrefersDark)) {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
})();

function toggleTheme() {
    const isDark = document.documentElement.classList.toggle('dark');
    localStorage.setItem('mark_graphiko_theme', isDark ? 'dark' : 'light');
    updateThemeIcons();
}

function updateThemeIcons() {
    const isDark = document.documentElement.classList.contains('dark');
    const sunIcons = document.querySelectorAll('.theme-sun-icon');
    const moonIcons = document.querySelectorAll('.theme-moon-icon');

    sunIcons.forEach(el => el.classList.toggle('hidden', !isDark));
    moonIcons.forEach(el => el.classList.toggle('hidden', isDark));
}

// 2. SCROLL REVEAL OBSERVER
function initScrollReveal() {
    const reveals = document.querySelectorAll('.reveal');
    if (!reveals.length) return;

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('active');
            }
        });
    }, {
        threshold: 0.12,
        rootMargin: '0px 0px -40px 0px'
    });

    reveals.forEach(el => observer.observe(el));
}

// 3. INTERACTIVE CAROUSEL CONTROLLER
function initCarousel(carouselId) {
    const container = document.getElementById(carouselId);
    if (!container) return;

    const track = container.querySelector('.carousel-track');
    const slides = container.querySelectorAll('.carousel-slide');
    const prevBtn = container.querySelector('.carousel-prev');
    const nextBtn = container.querySelector('.carousel-next');
    const dotsContainer = container.querySelector('.carousel-dots');

    if (!track || slides.length === 0) return;

    let currentIndex = 0;
    let autoPlayInterval = null;
    let startX = 0;
    let currentX = 0;
    let isSwiping = false;

    function getVisibleSlidesCount() {
        if (window.innerWidth >= 1024) return 3;
        if (window.innerWidth >= 768) return 2;
        return 1;
    }

    function maxIndex() {
        return Math.max(0, slides.length - getVisibleSlidesCount());
    }

    function createDots() {
        if (!dotsContainer) return;
        dotsContainer.innerHTML = '';
        const count = maxIndex() + 1;
        for (let i = 0; i < count; i++) {
            const dot = document.createElement('button');
            dot.className = `w-2.5 h-2.5 rounded-full transition-all duration-300 ${i === currentIndex ? 'bg-indigo-500 w-8' : 'bg-slate-400/40 dark:bg-slate-700 hover:bg-slate-500'}`;
            dot.setAttribute('aria-label', `Slide ${i + 1}`);
            dot.addEventListener('click', () => goToSlide(i));
            dotsContainer.appendChild(dot);
        }
    }

    function updateDots() {
        if (!dotsContainer) return;
        const dots = dotsContainer.querySelectorAll('button');
        dots.forEach((dot, idx) => {
            if (idx === currentIndex) {
                dot.className = 'w-8 h-2.5 rounded-full bg-indigo-500 transition-all duration-300';
            } else {
                dot.className = 'w-2.5 h-2.5 rounded-full bg-slate-400/40 dark:bg-slate-700 hover:bg-slate-500 transition-all duration-300';
            }
        });
    }

    function updateTrackPosition() {
        const slideWidthPercent = 100 / getVisibleSlidesCount();
        track.style.transform = `translateX(-${currentIndex * slideWidthPercent}%)`;
        updateDots();
    }

    function goToSlide(index) {
        currentIndex = Math.max(0, Math.min(index, maxIndex()));
        updateTrackPosition();
    }

    function nextSlide() {
        if (currentIndex >= maxIndex()) {
            currentIndex = 0;
        } else {
            currentIndex++;
        }
        updateTrackPosition();
    }

    function prevSlide() {
        if (currentIndex <= 0) {
            currentIndex = maxIndex();
        } else {
            currentIndex--;
        }
        updateTrackPosition();
    }

    function startAutoPlay() {
        stopAutoPlay();
        autoPlayInterval = setInterval(nextSlide, 5000);
    }

    function stopAutoPlay() {
        if (autoPlayInterval) {
            clearInterval(autoPlayInterval);
            autoPlayInterval = null;
        }
    }

    // Touch Swipe Support
    track.addEventListener('touchstart', (e) => {
        stopAutoPlay();
        startX = e.touches[0].clientX;
        isSwiping = true;
    }, { passive: true });

    track.addEventListener('touchmove', (e) => {
        if (!isSwiping) return;
        currentX = e.touches[0].clientX;
    }, { passive: true });

    track.addEventListener('touchend', () => {
        if (!isSwiping) return;
        const diffX = startX - currentX;
        if (diffX > 50) nextSlide();
        else if (diffX < -50) prevSlide();
        isSwiping = false;
        startAutoPlay();
    });

    if (prevBtn) prevBtn.addEventListener('click', () => { stopAutoPlay(); prevSlide(); startAutoPlay(); });
    if (nextBtn) nextBtn.addEventListener('click', () => { stopAutoPlay(); nextSlide(); startAutoPlay(); });

    container.addEventListener('mouseenter', stopAutoPlay);
    container.addEventListener('mouseleave', startAutoPlay);

    window.addEventListener('resize', () => {
        currentIndex = Math.min(currentIndex, maxIndex());
        createDots();
        updateTrackPosition();
    });

    createDots();
    updateTrackPosition();
    startAutoPlay();
}

// 4. HIGH-RESOLUTION MODAL HANDLER
function openDesignModal(id, title, category, imagePath, description, client, tools, tags) {
    const modal = document.getElementById('designModal');
    if (!modal) return;

    document.getElementById('modalTitle').textContent = title || 'Design Artwork';
    document.getElementById('modalCategory').textContent = category || 'Portfolio Piece';
    document.getElementById('modalImage').src = imagePath;
    document.getElementById('modalImage').alt = title;
    document.getElementById('modalDescription').textContent = description || 'High-resolution creative design piece created by Mark Graphiko.';
    document.getElementById('modalClient').textContent = client || 'Personal Concept Project';
    document.getElementById('modalTools').textContent = tools || 'Adobe Photoshop, Illustrator';
    
    // Direct Inquiry Link passing design reference ID
    const inquireBtn = document.getElementById('modalInquireBtn');
    if (inquireBtn) {
        inquireBtn.href = `/Message?designId=${id}`;
    }

    // Render Tags
    const tagsContainer = document.getElementById('modalTags');
    if (tagsContainer) {
        tagsContainer.innerHTML = '';
        if (tags) {
            tags.split(',').forEach(tag => {
                const badge = document.createElement('span');
                badge.className = 'px-2.5 py-1 text-xs font-medium rounded-md bg-indigo-500/10 text-indigo-500 dark:text-indigo-400 border border-indigo-500/20';
                badge.textContent = '#' + tag.trim();
                tagsContainer.appendChild(badge);
            });
        }
    }

    modal.classList.remove('hidden');
    document.body.style.overflow = 'hidden';
}

function closeDesignModal() {
    const modal = document.getElementById('designModal');
    if (!modal) return;
    modal.classList.add('hidden');
    document.body.style.overflow = '';
}

// 5. AJAX FAVORITE TOGGLE
async function toggleFavorite(designId, btnElement) {
    try {
        const response = await fetch('/Designs/ToggleFavorite', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ designId: designId })
        });

        if (response.status === 401) {
            window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
            return;
        }

        const data = await response.json();
        if (data.success) {
            const heartIcon = btnElement.querySelector('svg');
            if (data.isFavorited) {
                heartIcon.setAttribute('fill', 'currentColor');
                btnElement.classList.add('text-rose-500');
                btnElement.classList.remove('text-slate-400');
            } else {
                heartIcon.setAttribute('fill', 'none');
                btnElement.classList.remove('text-rose-500');
                btnElement.classList.add('text-slate-400');
            }
        }
    } catch (err) {
        console.error('Error toggling favorite:', err);
    }
}

// 6. GALLERY REAL-TIME FILTERING & SEARCH
function initGalleryFilters() {
    const searchInput = document.getElementById('gallerySearch');
    const filterButtons = document.querySelectorAll('.gallery-filter-btn');
    const cards = document.querySelectorAll('.gallery-card');

    if (!cards.length) return;

    let activeFilter = 'all';
    let searchQuery = '';

    function applyFilters() {
        cards.forEach(card => {
            const cardCategory = card.getAttribute('data-category') || '';
            const cardText = (card.getAttribute('data-search') || '').toLowerCase();

            const matchesCategory = activeFilter === 'all' || cardCategory.toLowerCase() === activeFilter.toLowerCase();
            const matchesSearch = !searchQuery || cardText.includes(searchQuery);

            if (matchesCategory && matchesSearch) {
                card.style.display = '';
                card.style.opacity = '1';
                card.style.transform = 'scale(1)';
            } else {
                card.style.display = 'none';
            }
        });

        // Check if empty
        const visibleCount = Array.from(cards).filter(c => c.style.display !== 'none').length;
        const emptyState = document.getElementById('galleryEmptyState');
        if (emptyState) {
            emptyState.classList.toggle('hidden', visibleCount > 0);
        }
    }

    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            searchQuery = e.target.value.trim().toLowerCase();
            applyFilters();
        });
    }

    filterButtons.forEach(btn => {
        btn.addEventListener('click', (e) => {
            filterButtons.forEach(b => {
                b.classList.remove('bg-indigo-600', 'text-white', 'shadow-lg');
                b.classList.add('bg-slate-200/60', 'dark:bg-slate-800/80', 'text-slate-700', 'dark:text-slate-300');
            });
            btn.classList.add('bg-indigo-600', 'text-white', 'shadow-lg');
            btn.classList.remove('bg-slate-200/60', 'dark:bg-slate-800/80', 'text-slate-700', 'dark:text-slate-300');

            activeFilter = btn.getAttribute('data-slug') || 'all';
            applyFilters();
        });
    });
}

// 7. INITIALIZE ON DOM READY
document.addEventListener('DOMContentLoaded', () => {
    updateThemeIcons();
    initScrollReveal();
    initCarousel('featuredCarousel');
    initGalleryFilters();

    // Auto-dismiss alert banners after 4 seconds
    setTimeout(() => {
        const alerts = document.querySelectorAll('.toast-alert');
        alerts.forEach(alert => {
            alert.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-10px)';
            setTimeout(() => alert.remove(), 500);
        });
    }, 4000);

    // Escape key closes modals
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeDesignModal();
            closeLogoutModal();
            closeChatDeleteModal();
        }
    });
});

// 8. LOGOUT CONFIRMATION MODAL CONTROLLER
function openLogoutModal() {
    const modal = document.getElementById('logoutModal');
    if (!modal) return;
    modal.classList.remove('hidden');
    document.body.style.overflow = 'hidden';
}

function closeLogoutModal() {
    const modal = document.getElementById('logoutModal');
    if (!modal) return;
    modal.classList.add('hidden');
    document.body.style.overflow = '';
}

// 9. CHAT ARCHIVE / PERMANENT DELETE MODAL CONTROLLER
function openChatDeleteModal(messageId, subjectTitle) {
    const modal = document.getElementById('chatDeleteModal');
    if (!modal) return;

    const titleEl = document.getElementById('chatDeleteSubject');
    if (titleEl) titleEl.textContent = subjectTitle || 'this project inquiry';

    const archiveForm = document.getElementById('modalArchiveForm');
    if (archiveForm) {
        archiveForm.action = archiveForm.getAttribute('data-base-action') + '/' + messageId;
    }

    const deleteForm = document.getElementById('modalPermanentDeleteForm');
    if (deleteForm) {
        deleteForm.action = deleteForm.getAttribute('data-base-action') + '/' + messageId;
    }

    modal.classList.remove('hidden');
    document.body.style.overflow = 'hidden';
}

function closeChatDeleteModal() {
    const modal = document.getElementById('chatDeleteModal');
    if (!modal) return;
    modal.classList.add('hidden');
    document.body.style.overflow = '';
}

