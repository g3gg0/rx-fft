// LibRXFFT_native.cpp : Definiert die exportierten Funktionen für die DLL-Anwendung.
//

#include "stdlib.h"
#include "math.h"
#include "LibRXFFT_native.h"

#define MAX(a,b) (((a)>=(b))?(a):(b))
#define MIN(a,b) (((a)<=(b))?(a):(b))

LIBRXFFT_NATIVE_API int *FIRInit(double *coeff, int entries)
{
	int pos = 0;
	FIRState *state = (FIRState *)malloc(sizeof(FIRState));

	state->Coefficients = (double*)malloc(sizeof(double) * entries);
	state->DelayLine = (double*)malloc(sizeof(double) * entries);

	for(pos = 0; pos < entries; pos++)
	{
		state->Coefficients[pos] = coeff[pos];
		state->DelayLine[pos] = 0;
	}

	state->DelayLinePosition = 0;
	state->Entries = entries;

	return (int *)state;
}

LIBRXFFT_NATIVE_API void FIRFree(int *ctx)
{
	FIRState *state = (FIRState *)ctx;

	free(state->Coefficients);
	free(state->DelayLine);
	free(state);
}

LIBRXFFT_NATIVE_API void FIRProcess(int *ctx, double *inData, double *outData, int samples)
{
	FIRState *state = (FIRState *)ctx;
	double result = 0.0f;
	double *delayLine = state->DelayLine;
	double *coeff = state->Coefficients;
	int count = state->DelayLinePosition;
	int index = count;
	int length = state->Entries;
	int pos = 0;
	int i = 0;

	for(pos = 0; pos < samples; pos++)
	{
		delayLine[count] = inData[pos];
		result = 0.0f;

		index = count;
		for (i = 0; i < length; i++) 
		{
			result += coeff[i] * delayLine[index--];
			if (index < 0) 
				index = length-1;
		}

		if (++count >= length) 
			count = 0;

		outData[pos] = result;
	}

	state->DelayLinePosition = count;
	return;
}



LIBRXFFT_NATIVE_API int *FMDemodInit()
{
	FMDemodState *state = (FMDemodState *)malloc(sizeof(FMDemodState));

	state->LastI = 0;
	state->LastQ = 0;

	return (int *)state;
}

LIBRXFFT_NATIVE_API void FMDemodFree(int *ctx)
{
	FMDemodState *state = (FMDemodState *)ctx;

	free(state);
}

LIBRXFFT_NATIVE_API void FMDemodProcess(int *ctx, double *iDataIn, double *qDataIn, double *outData, int samples)
{
	FMDemodState *state = (FMDemodState *)ctx;
	int pos = 0;

	for(pos = 0; pos < samples; pos++)
	{
		double iData = iDataIn[pos];
		double qData = qDataIn[pos];

        double norm = (iData * iData + qData * qData) * 4;
        double deltaI = iData - state->LastI;
        double deltaQ = qData - state->LastQ;

        double sampleValue = (iData * deltaQ - qData * deltaI) / norm;

        state->LastI = iData;
        state->LastQ = qData;

        outData[pos] = sampleValue;
	}
}



LIBRXFFT_NATIVE_API int *AMDemodInit()
{
	return (int *)0xDEADBEEF;
}

LIBRXFFT_NATIVE_API void AMDemodFree(int *ctx)
{
}

LIBRXFFT_NATIVE_API void AMDemodProcess(int *ctx, double *iDataIn, double *qDataIn, double *outData, int samples)
{
	int pos = 0;

	for(pos = 0; pos < samples; pos++)
	{
		double iData = iDataIn[pos];
		double qData = qDataIn[pos];

        outData[pos] = sqrt(iData * iData + qData * qData);
	}
}



LIBRXFFT_NATIVE_API int *DownmixInit(double *cosTable, double *sinTable, int entries)
{
	int pos = 0;
	DownmixState *state = (DownmixState *)malloc(sizeof(DownmixState));

	state->CosTable = (double *)malloc(sizeof(double) * entries);
	state->SinTable = (double *)malloc(sizeof(double) * entries);
	state->TableEntries = entries;
	state->TimePos = 0;

	for(pos = 0; pos < entries; pos++)
	{
		state->CosTable[pos] = cosTable[pos];
		state->SinTable[pos] = sinTable[pos];
	}

	return (int *)state;
}

LIBRXFFT_NATIVE_API void DownmixFree(int *ctx)
{
	DownmixState *state = (DownmixState *)ctx;

	free(state->CosTable);
	free(state->SinTable);
	free(state);
}

LIBRXFFT_NATIVE_API void DownmixProcess(int *ctx, double *iDataIn, double *qDataIn, double *iDataOut, double *qDataOut, int samples)
{
	DownmixState *state = (DownmixState *)ctx;
	int pos = 0;
	int timePos = state->TimePos;
	int length = state->TableEntries;
	double *cosTable = state->CosTable;
	double *sinTable = state->SinTable;

	for(pos = 0; pos < samples; pos++)
	{
		double iData = iDataIn[pos];
		double qData = qDataIn[pos];

		
        iDataOut[pos] = cosTable[timePos] * iData - sinTable[timePos] * qData;
        qDataOut[pos] = cosTable[timePos] * qData + sinTable[timePos] * iData;

        timePos++;
        timePos %= length;
	}
	
	state->TimePos = timePos;
}

int getIntFromBytes(unsigned char *readBuffer, int pos)
{
	int value = 0;
    if (readBuffer == NULL)
        return 0;

    value = (readBuffer[pos + 1] << 8) | readBuffer[pos];

    if (value > 0x7FFF)
        value = value - 0x10000;

    value = MAX(value, -0x7FFF);
    value = MIN(value, 0x7FFF);

    return value;
}

__inline double getDoubleFromBytes(unsigned char *readBuffer, int pos)
{
    return (double)(((short*)readBuffer)[pos/sizeof(short)]) / 0x7FFF;
    //return (double)getIntFromBytes(readBuffer, pos) / 0x7FFF;
}

LIBRXFFT_NATIVE_API void SamplesFromBinary(unsigned char *dataBuffer, int bytesRead, double *samplesI, double *samplesQ, int dataFormat, int invertedSpectrum)
{
    int bytesPerSample = 0;
    int bytesPerSamplePair = 0;
    int samplePos = 0;
    int samplePairs = 0;
	int pos = 0;

    switch (dataFormat)
    {
        case 0:
            bytesPerSamplePair = 4;
            bytesPerSample = 2;
            break;

        case 1:
        case 2:
            bytesPerSamplePair = 8;
            bytesPerSample = 4;
            break;

        default:
            bytesPerSamplePair = 0;
            bytesPerSample = 0;
            break;
    }

    samplePos = 0;
    samplePairs = bytesRead / bytesPerSamplePair;

    for (pos = 0; pos < samplePairs; pos++)
    {
        double I;
        double Q;
        switch (dataFormat)
        {
            case 0:
                I = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos);
                Q = getDoubleFromBytes(dataBuffer, bytesPerSamplePair * pos + bytesPerSample);
                break;

            case 1:
                I = ((float*)dataBuffer)[2 * pos];
                Q = ((float*)dataBuffer)[2 * pos + 1];
                break;

            case 2:
                I = ((float*)dataBuffer)[2 * pos] / 65536;
                Q = ((float*)dataBuffer)[2 * pos + 1] / 65536;
                break;

            default:
                return;
        }

        if (invertedSpectrum)
            I = -I;

        samplesI[pos] = I;
        samplesQ[pos] = Q;
    }

    return;
}
