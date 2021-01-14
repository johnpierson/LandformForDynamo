﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace LandformExample
{
    public class Topography
    {
        private Topography()
        {
        }

        public static List<Point> GetPoints(Revit.Elements.Topography topography)
        {
            var points = topography.Points;
            return points;
        }

        public static void DeletePoints(Revit.Elements.Topography topography, List<Point>pointsToRemove)
        {
            var internalTopography = topography.InternalElement as TopographySurface;
            //start transaction to make a change
            //TransactionManager.Instance.EnsureInTransaction(internalTopography.Document);
            TransactionManager.Instance.ForceCloseTransaction();
            //delete points
            TopographyEditScope editScope = new TopographyEditScope(internalTopography.Document, "Delete Points");
            editScope.Start(internalTopography.Id);
            internalTopography.DeletePoints(pointsToRemove.ToXyzs());
            editScope.Commit(new TopographyEditFailuresPreprocessorVerbose());
            //finish and commit
            //TransactionManager.Instance.TransactionTaskDone();
        }
    }
    class TopographyEditFailuresPreprocessorVerbose : IFailuresPreprocessor
    {
        // For debugging
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            try
            {
                TaskDialog.Show("Preprocess failures", "Hello");
                IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
                int numberOfFailures = failureMessages.Count;
                TaskDialog.Show("Preprocess failures", "Found " + numberOfFailures + " failure messages.");
                if (numberOfFailures < 5)
                {
                    foreach (FailureMessageAccessor msgAccessor in failureMessages)
                    {
                        TaskDialog.Show("Failure!", msgAccessor.GetDescriptionText());
                    }
                }
                else
                {
                    TaskDialog.Show("Failure 1 of " + numberOfFailures, failureMessages.First<FailureMessageAccessor>().GetDescriptionText());
                }
                TaskDialog.Show("Preprocess failures", "Goodbye");
                return FailureProcessingResult.Continue;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Exception", e.ToString());
                return FailureProcessingResult.ProceedWithRollBack;
            }
        }

    }
}
