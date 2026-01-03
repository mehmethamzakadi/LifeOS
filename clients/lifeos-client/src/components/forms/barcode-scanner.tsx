import { useRef, useState } from 'react';
import { BrowserMultiFormatReader, BarcodeFormat, DecodeHintType } from '@zxing/library';
import { Camera, Loader2, Image as ImageIcon, BookOpen, AlertCircle } from 'lucide-react';
import { Button } from '../ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../ui/dialog';

interface BarcodeScannerProps {
  onScan: (isbn: string) => void;
  onClose?: () => void;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function BarcodeScanner({ onScan, isOpen, onOpenChange }: BarcodeScannerProps) {
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // İki ayrı ref kullanıyoruz
  const cameraInputRef = useRef<HTMLInputElement>(null);
  const galleryInputRef = useRef<HTMLInputElement>(null);

  const processImage = async (file: File) => {
    if (!file) return;

    try {
      setIsProcessing(true);
      setError(null);

      const imageUrl = URL.createObjectURL(file);
      const hints = new Map();
      hints.set(DecodeHintType.POSSIBLE_FORMATS, [BarcodeFormat.EAN_13]);
      hints.set(DecodeHintType.TRY_HARDER, true);

      const codeReader = new BrowserMultiFormatReader(hints);

      try {
        const result = await codeReader.decodeFromImageUrl(imageUrl);
        const text = result.getText();
        
        console.log("Okunan kod:", text);

        if (text.length === 13 && (text.startsWith('978') || text.startsWith('979'))) {
          onScan(text);
          onOpenChange(false);
        } else {
          setError("Barkod okundu ancak geçerli bir kitap ISBN numarası (978...) değil.");
        }
      } catch (decodeErr) {
        console.error('Barkod okuma hatası:', decodeErr);
        setError("Fotoğrafta okunabilir bir ISBN barkodu bulunamadı. Lütfen daha net veya yakından çekip tekrar deneyin.");
      }

      URL.revokeObjectURL(imageUrl);
    } catch (err) {
      console.error("Genel hata:", err);
      setError("Dosya işlenirken hata oluştu.");
    } finally {
      setIsProcessing(false);
      // Inputları temizle ki aynı dosyayı tekrar seçebilsin
      if (cameraInputRef.current) cameraInputRef.current.value = '';
      if (galleryInputRef.current) galleryInputRef.current.value = '';
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) processImage(file);
  };

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md mx-4 select-none">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <BookOpen className="h-5 w-5" />
            Kitap Barkodu Tara
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-4">
          
          {/* 1. INPUT: Sadece Kamera (capture="environment" var) */}
          <input
            ref={cameraInputRef}
            type="file"
            accept="image/*"
            capture="environment" 
            className="hidden"
            onChange={handleFileChange}
          />

          {/* 2. INPUT: Sadece Galeri (capture YOK) */}
          <input
            ref={galleryInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={handleFileChange}
          />

          <div className="flex flex-col gap-3">
            <div className="text-center mb-4">
              <div className="mx-auto w-16 h-16 bg-muted rounded-full flex items-center justify-center mb-3">
                 <Camera className="h-8 w-8 opacity-50" />
              </div>
              <p className="text-sm text-muted-foreground px-4">
                Kitabın arkasındaki barkodun net bir fotoğrafını çekin veya galeriden yükleyin.
              </p>
            </div>

            {error && (
              <div className="flex items-start gap-2 p-3 bg-destructive/10 border border-destructive/20 rounded-lg">
                <AlertCircle className="h-4 w-4 text-destructive mt-0.5 shrink-0" />
                <p className="text-sm text-destructive">{error}</p>
              </div>
            )}

            {isProcessing ? (
              <Button disabled className="w-full h-12 text-lg">
                <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                İşleniyor...
              </Button>
            ) : (
              <>
                {/* Ana Buton: Kamera */}
                <Button 
                  size="lg" 
                  className="w-full h-12 text-lg gap-2 shadow-lg active:scale-95 transition-transform"
                  onClick={() => cameraInputRef.current?.click()}
                >
                  <Camera className="h-5 w-5" />
                  Fotoğraf Çek
                </Button>

                <div className="relative py-2">
                    <div className="absolute inset-0 flex items-center">
                        <span className="w-full border-t" />
                    </div>
                    <div className="relative flex justify-center text-xs uppercase">
                        <span className="bg-background px-2 text-muted-foreground">veya</span>
                    </div>
                </div>

                {/* İkincil Buton: Galeri */}
                <Button 
                  variant="outline" 
                  className="w-full h-12 text-lg gap-2"
                  onClick={() => galleryInputRef.current?.click()}
                >
                  <ImageIcon className="h-5 w-5" />
                  Galeriden Seç
                </Button>
              </>
            )}

            <div className="text-xs text-muted-foreground space-y-1 w-full pt-2 border-t">
              <p>• Mobilde: "Fotoğraf Çek" kamerayı açar</p>
              <p>• "Galeriden Seç" ile daha önce çektiğiniz fotoğrafları kullanabilirsiniz</p>
              <p>• Barkod net ve iyi aydınlatılmış olmalıdır</p>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
