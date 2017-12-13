using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;


namespace _3DReconstructionWPF.Data
{
    public class ProcessingStage
    {

        public enum Description
        {
            FeatureDetection,
            KinectStream,
            SkeletonArm
        }

        private Dictionary<ProcessingStage.Description, bool> _data;
        private Label _label;
        private ImageSource _cross;
        private ImageSource _tick;
        private List<Image> _images;

        public ProcessingStage(Label l, ImageSource cross,ImageSource tick, Image feature, Image kinect, Image skeleton)
        {
            _cross = cross;
            _tick = tick;

            _images = new List<Image>
            {
                feature,
                kinect,
                skeleton
            };
            foreach (var el in _images)
                el.Source = _cross;

            _label = l;
            _data = new Dictionary<Description, bool>
            {
                [ProcessingStage.Description.FeatureDetection] = false,
                [ProcessingStage.Description.KinectStream] = false,
                [ProcessingStage.Description.SkeletonArm] = false
            };

            
        }

        public bool GetProcessingStage(ProcessingStage.Description d)
        {
            return _data[d];
        }

        public void CompleteProcessingStage(ProcessingStage.Description id)
        {
            ChangeProcessingStage(id, true);
        }

        public void ChangeProcessingStage(ProcessingStage.Description id, bool b)
        {
            _data[id] = b;
            if (b) _images[(int)id].Source = _tick;
            else _images[(int)id].Source = _cross;

            if (CheckProcessingStage()) _label.Content = "completed";
            else _label.Content = "refreshing...";
        }

        private bool CheckProcessingStage()
        {
            bool result = true;
            foreach(KeyValuePair<ProcessingStage.Description,bool> entry in _data)
            {
                // do something with entry.Value or entry.Key
                result = result && entry.Value;
            }
            return result;
        }

    }
}

