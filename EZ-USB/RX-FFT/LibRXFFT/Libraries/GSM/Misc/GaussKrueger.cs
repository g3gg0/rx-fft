using System;

namespace LibRXFFT.Libraries.GSM.Misc
{
    public class GaussKrueger
    {
        /// <summary>
        /// Converts the gauss krueger to lat long.
        /// </summary>
        /// <param name="rw">The rw.</param>
        /// <param name="hw">The hw.</param>
        /// <param name="lg">The lg.</param>
        /// <param name="bg">The bg.</param>

        // from http://www.florianb.net/post/2007/08/13/Conversion-of-the-Gauss-Krueger-notation-into-latitudelongitude.aspx
        public static void ConvertToLatLong(double rw, double hw, out double lg, out double bg)
        {
            const double rho = 180 / Math.PI;
            double rm, e2, c, bI, bII, bf, co, g2, g1, t, fa, dl, gb, gl, grad, min, sek;
            int mKen;
            e2 = 0.0067192188;
            c = 6398786.849;
            mKen = Convert.ToInt32(Math.Floor(rw / 1000000f)); //Abrunden
            rm = rw - mKen * 1000000 - 500000;
            bI = hw / 10000855.7646;
            bII = bI * bI;
            bf = 325632.08677 * bI * ((((((0.00000562025 * bII - 0.00004363980) * bII + 0.00022976983) * bII - 0.00113566119) * bII + 0.00424914906) * bII - 0.00831729565) * bII + 1);
            bf = bf / 3600 / rho;
            co = Math.Cos(bf);
            g2 = e2 * (co * co);
            g1 = c / Math.Sqrt(1 + g2);
            t = Math.Tan(bf);
            fa = rm / g1;
            gb = bf - fa * fa * t * (1 + g2) / 2 + fa * fa * fa * fa * t * (5 + 3 * t * t + 6 * g2 - 6 * g2 * t * t) / 24;
            gb = gb * rho;
            dl = fa - fa * fa * fa * (1 + 2 * t * t + g2) / 6 + fa * fa * fa * fa * fa * (1 + 28 * t * t + 24 * t * t * t * t) / 120;
            gl = dl * rho / co + mKen * 3;
            lg = gl;
            bg = gb;
        }

        public static void HelmertTransformation(double in_lg, double in_bg, out double lg, out double bg)
        {
            double CartesianXMeters, CartesianYMeters, CartesianZMeters, n, aBessel = 6377397.155;
            double eeBessel = 0.0066743722296294277832, CartOutputXMeters, CartOutputYMeters, CartOutputZMeters;
            double ScaleFactor = 0.00000982, RotXRad = -7.16069806998785E-06, RotYRad = 3.56822869296619E-07;
            double RotZRad = 7.06858347057704E-06, ShiftXMeters = 591.28, ShiftYMeters = 81.35, ShiftZMeters = 396.39;
            double aWGS84 = 6378137, eeWGS84 = 0.0066943799;
            double Latitude, LatitudeIt;

            double bg_rad = (in_bg / 180f) * Math.PI;
            double lg_rad = (in_lg / 180f) * Math.PI;

            n = eeBessel * Math.Sin(bg_rad) * Math.Sin(bg_rad);
            n = 1 - n;
            n = Math.Sqrt(n);
            n = aBessel / n;
            CartesianXMeters = n * Math.Cos(bg_rad) * Math.Cos(lg_rad);
            CartesianYMeters = n * Math.Cos(bg_rad) * Math.Sin(lg_rad);
            CartesianZMeters = n * (1 - eeBessel) * Math.Sin(bg_rad);
/*
            CartOutputXMeters = (1 + ScaleFactor) * CartesianXMeters + RotZRad * CartesianYMeters - RotYRad * CartesianZMeters + ShiftXMeters;
            CartOutputYMeters = -RotZRad * CartesianXMeters + (1 + ScaleFactor) * CartesianYMeters + RotXRad * CartesianZMeters + ShiftYMeters;
            CartOutputZMeters = RotYRad * CartesianXMeters - RotXRad * CartesianYMeters + (1 + ScaleFactor) * CartesianZMeters + ShiftZMeters;
*/
            CartOutputXMeters = (1 + ScaleFactor) * CartesianXMeters - RotZRad * CartesianYMeters + RotYRad * CartesianZMeters + ShiftXMeters;
            CartOutputYMeters = RotZRad * CartesianXMeters + (1 + ScaleFactor) * CartesianYMeters - RotXRad * CartesianZMeters + ShiftYMeters;
            CartOutputZMeters = -RotYRad * CartesianXMeters + RotXRad * CartesianYMeters + (1 + ScaleFactor) * CartesianZMeters + ShiftZMeters;

            lg_rad = Math.Atan((CartOutputYMeters / CartOutputXMeters));

            Latitude = (CartOutputXMeters * CartOutputXMeters) + (CartOutputYMeters * CartOutputYMeters);
            Latitude = Math.Sqrt(Latitude);
            Latitude = CartOutputZMeters / Latitude;
            Latitude = Math.Atan(Latitude);
            LatitudeIt = 99999999;
            do
            {
                LatitudeIt = Latitude;

                n = 1 - eeWGS84 * Math.Sin(Latitude) * Math.Sin(Latitude);
                n = Math.Sqrt(n);
                n = aWGS84 / n;
                Latitude = CartOutputXMeters * CartOutputXMeters + CartOutputYMeters * CartOutputYMeters;
                Latitude = Math.Sqrt(Latitude);
                Latitude = (CartOutputZMeters + eeWGS84 * n * Math.Sin(LatitudeIt)) / Latitude;
                Latitude = Math.Atan(Latitude);
            }
            while (Math.Abs(Latitude - LatitudeIt) >= 0.000000000000001);

            bg = (Latitude / Math.PI) * 180;
            lg = (lg_rad / Math.PI) * 180;
        }
    }
}
