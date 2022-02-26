// LibRXFFT_native.cpp : Definiert die exportierten Funktionen für die DLL-Anwendung.
//


#include <stdlib.h>
#include <stdint.h>
#include <math.h>
#include <omp.h>

#include "LibRXFFT_native.h"

#include "kiss_fft.h"

#define MAX(a,b) (((a)>=(b))?(a):(b))
#define MIN(a,b) (((a)<=(b))?(a):(b))


LIBRXFFT_NATIVE_API int *FFTInit(int size, kiss_fft_cpx *inData, kiss_fft_cpx *outData)
{
	int pos = 0;
	kiss_fft_cfg ctx = kiss_fft_alloc_buffered(size,0,NULL,NULL, inData, outData);

	return (int *)ctx;
}

LIBRXFFT_NATIVE_API void FFTFree(int *ctx)
{
	kiss_fft_free(ctx);
}

LIBRXFFT_NATIVE_API void FFTProcess(int *ctx)
{
	kiss_fft_buffered((kiss_fft_cfg)ctx);
}


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

LIBRXFFT_NATIVE_API int *IIRInit(double gain, int section, double *num, double *den)
{
	int pos = 0;
	IIRState *state = (IIRState *)malloc(sizeof(IIRState));

	state->m1 = (double*)malloc(sizeof(double) * section);
	state->m2 = (double*)malloc(sizeof(double) * section);
	state->Num = (double*)malloc(sizeof(double) * section * 3);
	state->Den = (double*)malloc(sizeof(double) * section * 3);

	for(pos = 0; pos < section * 3; pos++)
	{
		state->Num[pos] = num[pos];
		state->Den[pos] = den[pos];
	}
	for(pos = 0; pos < section; pos++)
	{
		state->m1[pos] = 0;
		state->m2[pos] = 0;
	}
	state->Gain = gain;
	state->Section = section;

	return (int *)state;
}

LIBRXFFT_NATIVE_API void IIRFree(int *ctx)
{
	IIRState *state = (IIRState *)ctx;

	free(state->Den);
	free(state->Num);
	free(state->m1);
	free(state->m2);
	free(state);
}

LIBRXFFT_NATIVE_API void IIRProcess(int *ctx, double *inData, double *outData, int samples)
{
	IIRState *state = (IIRState *)ctx;
	double *m1 = state->m1;
	double *m2 = state->m2;
	double *localDen = state->Den;
	double *localNum = state->Num;
	double Gain = state->Gain;
	int Section = state->Section;
	int pos = 0;

	for(pos = 0; pos < samples; pos++)
	{
		double result = 0;

		int i = 0;
		int arrayPos = 0;
		double s0 = 0;

		s0 = Gain * inData[pos];

		for (i = 0; i < Section; i++)
		{
			result = (s0 * localNum[arrayPos + 0] + m1[i]) / localDen[arrayPos + 0];
			m1[i] = m2[i] + s0 * localNum[arrayPos + 1] - result * localDen[arrayPos + 1];
			m2[i] = s0 * localNum[arrayPos + 2] - result * localDen[arrayPos + 2];

			s0 = result;
			arrayPos += 3;
		}

		outData[pos] = result;
	}
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
	double lastI = state->LastI;
	double lastQ = state->LastQ;

	for(pos = 0; pos < samples; pos++)
	{
		double iData = iDataIn[pos];
		double qData = qDataIn[pos];
		double norm = (iData * iData + qData * qData) * 4;

		double deltaI = iData - lastI;
		double deltaQ = qData - lastQ;

		outData[pos] = (iData * deltaQ - qData * deltaI) / norm;

		lastI = iData;
		lastQ = qData;
	}

	state->LastI = lastI;
	state->LastQ = lastQ;
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

	for( pos = 0; pos < samples; pos++)
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
		/* keep timePos local for parallel processing */
		int localTimePos = (timePos + pos) % length;
		double iData = iDataIn[pos];
		double qData = qDataIn[pos];

		iDataOut[pos] = cosTable[localTimePos] * iData - sinTable[localTimePos] * qData;
		qDataOut[pos] = cosTable[localTimePos] * qData + sinTable[localTimePos] * iData;
	}

	timePos += samples;
	timePos %= length;

	state->TimePos = timePos;
}

__inline uint16_t getUInt16FromBytes(unsigned char* buffer, int pos)
{
	return (buffer[pos]) | (buffer[pos + 1] << 8);
}

__inline uint16_t getUInt16FromBytesSwapped(unsigned char* buffer, int pos)
{
	return (buffer[pos] << 8) | (buffer[pos + 1]);
}

__inline uint32_t getUInt24FromBytes(unsigned char* buffer, int pos)
{
	return (buffer[pos]) | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16);
}

__inline uint32_t getUInt24FromBytesSwapped(unsigned char* buffer, int pos)
{
	return (buffer[pos + 2]) | (buffer[pos + 1] << 8) | (buffer[pos] << 16);
}



__inline double getDoubleFromUInt16(unsigned char* buffer, int pos)
{
	return (double)(getUInt16FromBytes(buffer, 2 * pos));
}

__inline double getDoubleFromUInt16Swapped(unsigned char* buffer, int pos)
{
	return (double)(getUInt16FromBytesSwapped(buffer, 2 * pos));
}

__inline double getDoubleFrom24Bit(unsigned char* buffer, int pos)
{
	return (double)(getUInt24FromBytes(buffer, 3 * pos));
}

__inline double getDoubleFrom24BitSwapped(unsigned char* buffer, int pos)
{
	return (double)(getUInt24FromBytesSwapped(buffer, 3 * pos));
}

__inline double getDoubleFromFloat(unsigned char* buffer, int pos)
{
	return (double)((float*)buffer)[pos];
}

__inline void setUInt16ToBytes(uint16_t value, unsigned char* buffer, int pos)
{
	buffer[pos + 0] = value & 0xFF;
	buffer[pos + 1] = (value >> 8) & 0xFF;
}

__inline void setUInt16ToBytesSwapped(uint16_t value, unsigned char* buffer, int pos)
{
	buffer[pos + 0] = (value >> 8) & 0xFF;
	buffer[pos + 1] = value & 0xFF;
}

__inline void setUInt24ToBytes(uint32_t value, unsigned char* buffer, int pos)
{
	buffer[pos + 0] = value & 0xFF;
	buffer[pos + 1] = (value >> 8) & 0xFF;
	buffer[pos + 2] = (value >> 16) & 0xFF;
}

__inline void setUInt24ToBytesSwapped(uint32_t value, unsigned char* buffer, int pos)
{
	buffer[pos + 0] = (value >> 16) & 0xFF;
	buffer[pos + 1] = (value >> 8) & 0xFF;
	buffer[pos + 2] = value & 0xFF;
}

__inline void setDoubleToUInt16(double value, unsigned char* buffer, int pos)
{
	setUInt16ToBytes((uint16_t)value, buffer, 2 * pos);
}

__inline void setDoubleToUInt16Swapped(double value, unsigned char* buffer, int pos)
{
	setUInt16ToBytesSwapped((uint16_t)value, buffer, 2 * pos);
}

__inline void setDoubleTo24Bit(double value, unsigned char* buffer, int pos)
{
	setUInt24ToBytes((uint32_t)value, buffer, 3 * pos);
}

__inline void setDoubleTo24BitSwapped(double value, unsigned char* buffer, int pos)
{
	setUInt24ToBytesSwapped((uint32_t)value, buffer, 3 * pos);
}

__inline void setDoubleToFloat(double value, unsigned char* buffer, int pos)
{
	((float*)buffer)[pos] = (float)value;
}



LIBRXFFT_NATIVE_API void SamplesFromBinary(unsigned char* dataBuffer, int bytesRead, int destSize, double* samplesI, double* samplesQ, int dataFormat, int invertedSpectrum)
{
	int bytesPerSamplePair = 0;

	switch (dataFormat)
	{
	case 0:
	case 1:
		bytesPerSamplePair = 4;
		break;

	case 2:
	case 3:
		bytesPerSamplePair = 6;
		break;

	case 4:
	case 5:
		bytesPerSamplePair = 8;
		break;

	default:
		bytesPerSamplePair = 0;
		break;
	}

	if (destSize == 0)
	{
		printf("SamplesFromBinary: ERROR - destSize is zero\r\n");
		return;
	}

	if (bytesPerSamplePair == 0)
	{
		printf("SamplesFromBinary: ERROR - bytesPerSamplePair is zero\r\n");
		return;
	}

	if ((bytesRead % bytesPerSamplePair) != 0)
	{
		printf("SamplesFromBinary: ERROR - buffer does not contain an even amount of values\r\n");
		return;
	}

	int samplePairs = bytesRead / bytesPerSamplePair;

	if (samplePairs > destSize)
	{
		printf("SamplesFromBinary: ERROR - too many bytes to read\r\n");
		return;
	}

	double divI = 1;
	double divQ = 1;
	int pos = 0;

	if (invertedSpectrum)
	{
		divI *= -1;
	}

	switch (dataFormat)
	{
			/* Direct16BitIQFixedPointLE */
		case 0:
			divI *= 0x7FFF;
			divQ *= 0x7FFF;
#pragma omp parallel
#pragma omp for
			for (pos = 0; pos < samplePairs; pos++)
			{
				samplesI[pos] = getDoubleFromUInt16(dataBuffer, 2 * pos) / divI;
				samplesQ[pos] = getDoubleFromUInt16(dataBuffer, 2 * pos + 1) / divQ;
			}

			break;

			/* Direct16BitIQFixedPointBE */
		case 1:
			divI *= 0x7FFF;
			divQ *= 0x7FFF;
#pragma omp parallel
#pragma omp for
			for (pos = 0; pos < samplePairs; pos++)
			{
				samplesI[pos] = getDoubleFromUInt16Swapped(dataBuffer, 2 * pos) / divI;
				samplesQ[pos] = getDoubleFromUInt16Swapped(dataBuffer, 2 * pos + 1) / divQ;
			}

			break;

			/* Direct24BitIQFixedPointLE */
		case 2:
			divI *= 0x7FFFFF;
			divQ *= 0x7FFFFF;
#pragma omp parallel
#pragma omp for
			for (pos = 0; pos < samplePairs; pos++)
			{
				samplesI[pos] = getDoubleFrom24Bit(dataBuffer, 2 * pos) / divI;
				samplesQ[pos] = getDoubleFrom24Bit(dataBuffer, 2 * pos + 1) / divQ;
			}

			break;

			/* Direct24BitIQFixedPointBE */
		case 3:
			divI *= 0x7FFFFF;
			divQ *= 0x7FFFFF;
#pragma omp parallel
#pragma omp for
			for (pos = 0; pos < samplePairs; pos++)
			{
				samplesI[pos] = getDoubleFrom24BitSwapped(dataBuffer, 2 * pos) / divI;
				samplesQ[pos] = getDoubleFrom24BitSwapped(dataBuffer, 2 * pos + 1) / divQ;
			}

			break;

			/* Direct32BitIQFloat */
		case 4:
#pragma omp parallel
#pragma omp for
			for (pos = 0; pos < samplePairs; pos++)
			{
				samplesI[pos] = getDoubleFromFloat(dataBuffer, 2 * pos) / divI;
				samplesQ[pos] = getDoubleFromFloat(dataBuffer, 2 * pos + 1) / divQ;
			}

			break;

			/* Direct32BitIQFloat64k */
		case 5:
			divI *= 0xFFFF;
			divQ *= 0xFFFF;
#pragma omp parallel
#pragma omp for
			for (pos = 0; pos < samplePairs; pos++)
			{
				samplesI[pos] = getDoubleFromFloat(dataBuffer, 2 * pos) / divI;
				samplesQ[pos] = getDoubleFromFloat(dataBuffer, 2 * pos + 1) / divQ;
			}

			break;

		default:
			break;
	}

	return;
}

LIBRXFFT_NATIVE_API void SamplesToBinary(unsigned char *dataBuffer, int samplePairs, double *samplesI, double *samplesQ, int dataFormat, int invertedSpectrum)
{
	int bytesPerSamplePair = 0;

	switch (dataFormat)
	{
		case 0:
		case 1:
			bytesPerSamplePair = 4;
			break;

		case 2:
		case 3:
			bytesPerSamplePair = 6;
			break;

		case 4:
		case 5:
			bytesPerSamplePair = 8;
			break;

		default:
			bytesPerSamplePair = 0;
			break;
	}

	if (bytesPerSamplePair == 0)
	{
		printf("SamplesFromBinary: ERROR - bytesPerSamplePair is zero\r\n");
		return;
	}

	double divI = 1;
	double divQ = 1;
	int pos = 0;

	if (invertedSpectrum)
	{
		divI *= -1;
	}

	switch (dataFormat)
	{
		/* Direct16BitIQFixedPointLE */
	case 0:
		divI *= 0x7FFF;
		divQ *= 0x7FFF;
#pragma omp parallel
#pragma omp for
		for (pos = 0; pos < samplePairs; pos++)
		{
			setDoubleToUInt16(samplesI[pos] * divI, dataBuffer, 2 * pos);
			setDoubleToUInt16(samplesQ[pos] * divQ, dataBuffer, 2 * pos + 1);
		}

		break;

		/* Direct16BitIQFixedPointBE */
	case 1:
		divI *= 0x7FFF;
		divQ *= 0x7FFF;
#pragma omp parallel
#pragma omp for
		for (pos = 0; pos < samplePairs; pos++)
		{
			setDoubleToUInt16Swapped(samplesI[pos] * divI, dataBuffer, 2 * pos);
			setDoubleToUInt16Swapped(samplesQ[pos] * divQ, dataBuffer, 2 * pos + 1);
		}

		break;

		/* Direct24BitIQFixedPointLE */
	case 2:
		divI *= 0x7FFFFF;
		divQ *= 0x7FFFFF;
#pragma omp parallel
#pragma omp for
		for (pos = 0; pos < samplePairs; pos++)
		{
			setDoubleTo24Bit(samplesI[pos] * divI, dataBuffer, 2 * pos);
			setDoubleTo24Bit(samplesQ[pos] * divQ, dataBuffer, 2 * pos + 1);
		}

		break;

		/* Direct24BitIQFixedPointBE */
	case 3:
		divI *= 0x7FFFFF;
		divQ *= 0x7FFFFF;
#pragma omp parallel
#pragma omp for
		for (pos = 0; pos < samplePairs; pos++)
		{
			setDoubleTo24BitSwapped(samplesI[pos] * divI, dataBuffer, 2 * pos);
			setDoubleTo24BitSwapped(samplesQ[pos] * divQ, dataBuffer, 2 * pos + 1);
		}

		break;

		/* Direct32BitIQFloat */
	case 4:
#pragma omp parallel
#pragma omp for
		for (pos = 0; pos < samplePairs; pos++)
		{
			setDoubleToFloat(samplesI[pos] * divI, dataBuffer, 2 * pos);
			setDoubleToFloat(samplesQ[pos] * divQ, dataBuffer, 2 * pos + 1);
		}

		break;

		/* Direct32BitIQFloat64k */
	case 5:
		divI *= 0xFFFF;
		divQ *= 0xFFFF;
#pragma omp parallel
#pragma omp for
		for (pos = 0; pos < samplePairs; pos++)
		{
			 setDoubleToFloat(samplesI[pos] * divI, dataBuffer, 2 * pos);
			 setDoubleToFloat(samplesQ[pos] * divQ, dataBuffer, 2 * pos + 1);
		}

		break;

	default:
		break;
	}

	return;
}
