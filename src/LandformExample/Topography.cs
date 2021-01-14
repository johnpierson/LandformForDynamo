using System;
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
            var doc = internalTopography.Document;

            //force close the dynamo transaction
            TransactionManager.Instance.ForceCloseTransaction();

            //start an edit scope
            TopographyEditScope editScope = new TopographyEditScope(doc, "Delete Points");
            editScope.Start(internalTopography.Id);

            //start transaction to make a change
            Transaction transaction = new Transaction(doc);
            transaction.Start("Start deleting points.");

            //delete points
            internalTopography.DeletePoints(pointsToRemove.ToXyzs());

            //finish and commit
            transaction.Commit();

            //commit the edit
            editScope.Commit(new TopographyEditFailuresPreprocessorVerbose());
        }
    }
    class TopographyEditFailuresPreprocessorVerbose : IFailuresPreprocessor
    {
        // For debugging
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            try
            {
                return FailureProcessingResult.Continue;
            }
            catch (Exception e)
            {
                return FailureProcessingResult.ProceedWithRollBack;
            }
        }

    }
}
