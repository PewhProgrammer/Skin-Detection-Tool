using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pointmatcher.net;

namespace _3DReconstructionWPF.Computation
{
    class ICP
    {
        EuclideanTransform transform;

        public struct ICPData
        {
            public DataPoints data;
            public EuclideanTransform transform;

            public ICPData(DataPoints d, EuclideanTransform e)
            {
                data = d;
                transform = e;
            }
        }

        public ICP()
        {

        }

        //using pointmatcher.net
        public ICPData ComputeICP(DataPoints reading, DataPoints reference, EuclideanTransform initialTransform)
        {
            // initialize your point cloud reading here
            // initialize your reference point cloud here
            // your initial guess at the transform from reading to reference

            pointmatcher.net.ICP icp = new pointmatcher.net.ICP
            {
                ReadingDataPointsFilters = new RandomSamplingDataPointsFilter(prob: 0.1f),
                ReferenceDataPointsFilters = new SamplingSurfaceNormalDataPointsFilter(SamplingMethod.RandomSampling, ratio: 0.2f),
                OutlierFilter   = new TrimmedDistOutlierFilter(ratio: 0.58f) // [0 - 1]
            }; 
            
            transform = icp.Compute(reading, reference, initialTransform);
            return new ICPData(ApplyTransformation(transform, reading), transform);
        }

        private DataPoints ApplyTransformation(EuclideanTransform eTransform,DataPoints reading)
        {
            Log.writeLog("Applying Translation: " + eTransform.translation.ToString());
            Log.writeLog("Applying Rotation: " + eTransform.rotation.ToString());

            for (int i = 0; i < reading.points.Length; i++)
            {
                reading.points[i].point = eTransform.Apply(reading.points[i].point);
            }

            return reading;
        }

        public void resetTransform()
        {
            transform = new EuclideanTransform();
        }
    }
}
