export function HomePage() {
  return (
    <div className="bg-background">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="space-y-12">
          <header className="space-y-3">
            <h1 className="text-3xl font-semibold text-foreground sm:text-4xl">LifeOS</h1>
            <p className="max-w-2xl text-sm text-muted-foreground sm:text-base">
              Modern, ölçeklenebilir ve güvenli bir proje temeli.
            </p>
          </header>

          <section className="space-y-6">
            <div className="rounded-2xl border border-border/60 bg-card p-10 text-center">
              <h2 className="text-2xl font-semibold text-foreground mb-4">Hoş Geldiniz</h2>
              <p className="text-sm text-muted-foreground max-w-2xl mx-auto">
                Bu, LifeOS'in temel bir proje şablonudur. Clean Architecture ve Domain-Driven Design prensiplerine dayalı olarak geliştirilmiştir. 
                Yeni projeleriniz için başlangıç noktası olarak kullanabileceğiniz, tam özellikli bir temel projedir.
              </p>
            </div>
          </section>
        </div>
      </div>
    </div>
  );
}
