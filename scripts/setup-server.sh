#!/bin/bash

# ============================================
# LifeOS - Ubuntu 22.04 Server Setup Script
# ============================================
# Bu script Ubuntu 22.04 VDS sunucusunu
# LifeOS deployment için hazırlar
# ============================================

set -e

# Renkler
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Root kontrolü
if [ "$EUID" -ne 0 ]; then
    print_error "Bu script root olarak çalıştırılmalıdır!"
    print_info "Kullanım: sudo bash setup-server.sh"
    exit 1
fi

echo "============================================"
echo "  LifeOS - Server Setup Script"
echo "============================================"
echo

# 1. Sistem Güncellemesi
print_info "Sistem güncelleniyor..."
apt update && apt upgrade -y
print_success "Sistem güncellendi"

# 2. Temel Paketler
print_info "Temel paketler yükleniyor..."
apt install -y curl wget git ufw fail2ban htop nano jq
print_success "Temel paketler yüklendi"

# 3. Docker Kurulumu
print_info "Docker kurulumu kontrol ediliyor..."
if command -v docker &> /dev/null; then
    print_warning "Docker zaten kurulu"
    docker --version
else
    print_info "Docker yükleniyor..."
    
    # Eski Docker versiyonlarını kaldır
    apt remove -y docker docker-engine docker.io containerd runc 2>/dev/null || true
    
    # Eski Docker repository dosyalarını temizle
    rm -f /etc/apt/sources.list.d/docker*.list 2>/dev/null || true
    rm -f /etc/apt/keyrings/docker.gpg 2>/dev/null || true
    
    # Docker için gerekli paketler
    apt install -y ca-certificates curl gnupg lsb-release
    
    # Ubuntu kod adını al
    UBUNTU_CODENAME=$(lsb_release -cs)
    ARCH=$(dpkg --print-architecture)
    
    print_info "Ubuntu versiyonu: $UBUNTU_CODENAME, Mimari: $ARCH"
    
    # Docker GPG key (daha güvenli yöntem)
    print_info "Docker GPG key ekleniyor..."
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    chmod a+r /etc/apt/keyrings/docker.gpg
    
    # Docker repository ekle
    print_info "Docker repository ekleniyor..."
    echo \
      "deb [arch=${ARCH} signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
      ${UBUNTU_CODENAME} stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
    
    # Paket listelerini güncelle
    print_info "Paket listeleri güncelleniyor..."
    if ! apt update 2>&1 | tee /tmp/docker-apt-update.log; then
        print_error "apt update başarısız oldu!"
        print_warning "Hata logları:"
        tail -20 /tmp/docker-apt-update.log
        
        # Eski repository dosyasını temizle ve tekrar dene
        print_info "Repository dosyası temizleniyor ve alternatif yöntem deneniyor..."
        rm -f /etc/apt/sources.list.d/docker.list
        
        # Ubuntu'nun kendi repository'sinden kur (alternatif)
        print_info "Ubuntu repository'sinden Docker yükleniyor (docker.io)..."
        apt update
        if apt install -y docker.io docker-compose; then
            systemctl start docker
            systemctl enable docker
            
            if command -v docker &> /dev/null; then
                print_success "Docker docker.io paketinden yüklendi"
            else
                print_error "Docker kurulumu başarısız oldu!"
                print_info "Lütfen manuel olarak kurun: https://docs.docker.com/engine/install/ubuntu/"
                exit 1
            fi
        else
            print_error "Docker kurulumu başarısız oldu!"
            print_info "Lütfen manuel olarak kurun: https://docs.docker.com/engine/install/ubuntu/"
            exit 1
        fi
    else
        # Docker yükle
        print_info "Docker paketleri yükleniyor..."
        if apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin; then
            # Docker servisi
            systemctl start docker
            systemctl enable docker
            
            # Kurulumu test et
            if docker --version &> /dev/null; then
                print_success "Docker yüklendi"
                docker --version
            else
                print_error "Docker yüklendi ama çalışmıyor!"
                exit 1
            fi
        else
            print_error "Docker paketleri yüklenemedi!"
            print_info "Repository dosyası temizleniyor ve alternatif yöntem deneniyor..."
            rm -f /etc/apt/sources.list.d/docker.list
            
            # Alternatif: Ubuntu'nun kendi repository'sinden kur
            print_info "Ubuntu repository'sinden Docker yükleniyor (docker.io)..."
            apt update
            if apt install -y docker.io docker-compose; then
                systemctl start docker
                systemctl enable docker
                
                if command -v docker &> /dev/null; then
                    print_success "Docker docker.io paketinden yüklendi"
                else
                    print_error "Docker kurulumu başarısız oldu!"
                    print_info "Lütfen manuel olarak kurun: https://docs.docker.com/engine/install/ubuntu/"
                    exit 1
                fi
            else
                print_error "Docker kurulumu başarısız oldu!"
                print_info "Lütfen manuel olarak kurun: https://docs.docker.com/engine/install/ubuntu/"
                exit 1
            fi
        fi
    fi
fi

# 4. Docker Compose kontrolü
print_info "Docker Compose kontrol ediliyor..."
if docker compose version &> /dev/null 2>&1; then
    print_success "Docker Compose mevcut (plugin)"
    docker compose version
elif command -v docker-compose &> /dev/null && docker-compose --version &> /dev/null 2>&1; then
    print_warning "Docker Compose mevcut (standalone)"
    docker-compose --version
    print_info "Not: Docker Compose plugin kullanmanız önerilir, ancak standalone versiyon da çalışır"
else
    print_warning "Docker Compose bulunamadı, yükleniyor..."
    
    # Docker Compose'u yükle
    apt update
    if apt install -y docker-compose-plugin 2>/dev/null; then
        print_success "Docker Compose plugin yüklendi"
        docker compose version
    elif apt install -y docker-compose 2>/dev/null; then
        print_success "Docker Compose (standalone) yüklendi"
        docker-compose --version
    else
        print_error "Docker Compose yüklenemedi!"
        print_info "Uygulama docker-compose komutuyla çalışacak, plugin kurulumu için manuel yükleme yapın"
        print_info "Detaylar: https://docs.docker.com/compose/install/"
    fi
fi

# 5. Firewall (UFW) Yapılandırması
print_info "Firewall yapılandırılıyor..."
ufw --force reset
ufw default deny incoming
ufw default allow outgoing
ufw allow 22/tcp comment 'SSH'
ufw allow 80/tcp comment 'HTTP'
ufw allow 443/tcp comment 'HTTPS'
ufw --force enable
print_success "Firewall yapılandırıldı"

# 6. Fail2Ban Yapılandırması
print_info "Fail2Ban yapılandırılıyor..."
cat > /etc/fail2ban/jail.local << EOF
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 5

[sshd]
enabled = true
port = 22
filter = sshd
logpath = /var/log/auth.log
maxretry = 3
bantime = 86400
EOF

systemctl restart fail2ban
systemctl enable fail2ban
print_success "Fail2Ban yapılandırıldı"

# 7. Certbot (SSL için)
print_info "Certbot yükleniyor..."
if command -v certbot &> /dev/null; then
    print_warning "Certbot zaten kurulu"
else
    apt install -y certbot python3-certbot-nginx
    print_success "Certbot yüklendi"
fi

# 8. Non-root user oluşturma (isteğe bağlı)
echo
read -p "Non-root user oluşturmak istiyor musunuz? (önerilir) (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    read -p "Kullanıcı adı girin (varsayılan: lifeos): " username
    username=${username:-lifeos}
    
    if id "$username" &>/dev/null; then
        print_warning "Kullanıcı $username zaten mevcut"
    else
        useradd -m -s /bin/bash $username
        usermod -aG sudo $username
        usermod -aG docker $username
        print_success "Kullanıcı $username oluşturuldu ve docker grubuna eklendi"
        
        echo
        print_warning "Kullanıcı için şifre belirleyin:"
        passwd $username
    fi
fi

# 9. Proje klasörü oluşturma
echo
read -p "Proje klasörü oluşturmak istiyor musunuz? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    read -p "Klasör yolu girin (varsayılan: /opt/lifeos): " project_dir
    project_dir=${project_dir:-/opt/lifeos}
    
    mkdir -p $project_dir
    if [ ! -z "$username" ]; then
        chown -R $username:$username $project_dir
        print_success "Klasör $project_dir oluşturuldu ve $username kullanıcısına verildi"
    else
        print_success "Klasör $project_dir oluşturuldu"
    fi
fi

echo
print_success "Server setup tamamlandı!"
echo
print_info "Sonraki adımlar:"
echo "  1. Proje dosyalarını sunucuya aktarın"
echo "  2. .env dosyasını oluşturun ve doldurun"
echo "  3. SSL sertifikası kurun (isteğe bağlı)"
echo "  4. deploy-production.sh scriptini çalıştırın"
echo
print_info "Detaylı bilgi için DEPLOYMENT.md dosyasına bakın"

