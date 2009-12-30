// Folgender ifdef-Block ist die Standardmethode zum Erstellen von Makros, die das Exportieren 
// aus einer DLL vereinfachen. Alle Dateien in dieser DLL werden mit dem LIBRXFFT_NATIVE_EXPORTS-Symbol
// kompiliert, das in der Befehlszeile definiert wurde. Das Symbol darf nicht für ein Projekt definiert werden,
// das diese DLL verwendet. Alle anderen Projekte, deren Quelldateien diese Datei beinhalten, erkennen 
// LIBRXFFT_NATIVE_API-Funktionen als aus einer DLL importiert, während die DLL
// mit diesem Makro definierte Symbole als exportiert ansieht.
#ifdef LIBRXFFT_NATIVE_EXPORTS
#define LIBRXFFT_NATIVE_API __declspec(dllexport)
#else
#define LIBRXFFT_NATIVE_API __declspec(dllimport)
#endif


typedef struct 
{
	double *Coefficients;
	double *DelayLine;
	int DelayLinePosition;
	int Entries;
} FIRState;

typedef struct 
{
	double *Num;
	double *Den;
	double Gain;
	int Section;
	double *m1;
	double *m2;
} IIRState;


typedef struct 
{
	double LastI;
	double LastQ;
} FMDemodState;


typedef struct 
{
	double *CosTable;
	double *SinTable;
	int TimePos;
	int TableEntries;
} DownmixState;

LIBRXFFT_NATIVE_API int *FIRInit(double *coeff, int entries);
LIBRXFFT_NATIVE_API void FIRFree(int *ctx);
LIBRXFFT_NATIVE_API void FIRProcess(int *ctx, double *inData, double *outData, int samples);

LIBRXFFT_NATIVE_API int *IIRInit(double gain, int section, double *num, double *den);
LIBRXFFT_NATIVE_API void IIRFree(int *ctx);
LIBRXFFT_NATIVE_API void IIRProcess(int *ctx, double *inData, double *outData, int samples);

LIBRXFFT_NATIVE_API int *FMDemodInit();
LIBRXFFT_NATIVE_API void FMDemodFree(int *ctx);
LIBRXFFT_NATIVE_API void FMDemodProcess(int *ctx, double *iDataIn, double *qDataIn, double *outData, int samples);

LIBRXFFT_NATIVE_API int *AMDemodInit();
LIBRXFFT_NATIVE_API void AMDemodFree(int *ctx);
LIBRXFFT_NATIVE_API void AMDemodProcess(int *ctx, double *iDataIn, double *qDataIn, double *outData, int samples);

LIBRXFFT_NATIVE_API int *DownmixInit(double *cosTable, double *sinTable, int entries);
LIBRXFFT_NATIVE_API void DownmixFree(int *ctx);
LIBRXFFT_NATIVE_API void DownmixProcess(int *ctx, double *iDataIn, double *qDataIn, double *iDataOut, double *qDataOut, int samples);

LIBRXFFT_NATIVE_API void SamplesFromBinary(unsigned char *dataBuffer, int bytesRead, double *samplesI, double *samplesQ, int dataFormat, int invertedSpectrum);
