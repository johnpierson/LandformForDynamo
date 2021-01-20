using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Dynamo.Graph.Nodes;
using Revit.GeometryConversion;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace LandformExample
{
    /// <summary>
    /// Wrapper for topography
    /// </summary>
    public class Topography
    {
        //to hide the overall topography class
        private Topography()
        {
        }

        /// <summary>
        /// This returns the topo points
        /// </summary>
        /// <param name="topography">The topography to get points from.</param>
        /// <returns name="points">The topography points.</returns>
        /// <search>
        /// Topography.Points
        /// </search>
        [NodeCategory("Query")]
        public static List<Point> GetPoints(Revit.Elements.Topography topography)
        {
            //this example cheats and uses the built in Revit libraries to get the points
            var points = topography.Points;
            return points;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topography"></param>
        /// <returns></returns>
        public static List<Point> InteriorPoints(Revit.Elements.Topography topography)
        {
            //cast the Revit.Elements.Topography to the Autodesk.Revit.DB.TopographySurface version
            var internalTopography = topography.InternalElement as TopographySurface;

            //get the interior points, convert to list and cast as Dynamo points
            return internalTopography.GetInteriorPoints().ToList().ToPoints();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topography"></param>
        /// <returns></returns>
        //NOTE: This example demonstrates how to iterate through a list and convert, in the off-chance that there is not a converter.
        public static List<Point> BoundaryPoints(Revit.Elements.Topography topography)
        {
            //cast the Revit.Elements.Topography to the Autodesk.Revit.DB.TopographySurface version
            var internalTopography = topography.InternalElement as TopographySurface;

            //our list to append dynamo points to
            List<Point> boundaryPoints = new List<Point>();

            //get the Autodesk.Revit.DB.XYZ points
            var revitPoints = internalTopography.GetBoundaryPoints();

            //iterate through and convert to dynamo points and add to our output list
            foreach (var pt in revitPoints)
            {
                boundaryPoints.Add(pt.ToPoint());
            }

            return boundaryPoints;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topography"></param>
        /// <param name="pointsToRemove"></param>
        public static void DeletePoints(Revit.Elements.Topography topography, List<Point> pointsToRemove)
        {
            //cast the Revit.Elements.Topography to the Autodesk.Revit.DB.TopographySurface version
            var internalTopography = topography.InternalElement as TopographySurface;
            //get the document related to the topography
            //TIP: (this method is useful because it retrieves the related document rather than just the current one)
            var doc = internalTopography.Document;

            /*TIP: you can also get the current active document with this built-in dynamo method
            var doc = DocumentManager.Instance.CurrentDBDocument*/

            //force close the dynamo transaction
            TransactionManager.Instance.ForceCloseTransaction();

            //start a topography edit scope
            TopographyEditScope editScope = new TopographyEditScope(doc, "Delete Points");
            editScope.Start(internalTopography.Id);

            //create and start a transaction to make a change to the topography
            Transaction transaction = new Transaction(doc);
            transaction.Start("Start deleting points.");

            //delete points - ToXyzs() will convert Dynamo points to Autodesk.Revit.DB.Point equivalents
            internalTopography.DeletePoints(pointsToRemove.ToXyzs());

            //finish and commit the transaction
            transaction.Commit();

            //commit the edit
            editScope.Commit(new TopographyEditFailuresPreprocessorVerbose());


            //set the result in appdomain
            List<object> newData = new List<object> { DateTime.Now, "Succesfully deleted points.", topography };
            SetData(newData);
        }

        public static List<List<object>> TopographyEditResults(bool toggle = true)
        {
            return AppDomain.CurrentDomain.GetData("Landform Results") as List<List<object>>;
        }

        private static void SetData(List<object> data)
        {
            if (AppDomain.CurrentDomain.GetData("Landform Results") is null)
            {
                List<List<object>> newData = new List<List<object>> { data };
                AppDomain.CurrentDomain.SetData("Landform Results", newData);
            }
            else
            {
                List<List<object>> currentData = AppDomain.CurrentDomain.GetData("Landform Results") as List<List<object>>;
                currentData.Add(data);

                AppDomain.CurrentDomain.SetData("Landform Results", currentData);
            }
        }
    }

    #region Helpers
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
                TaskDialog.Show("Error", $"The edit failed. Here is the error message from Revit: {e.Message}");
                return FailureProcessingResult.ProceedWithRollBack;
            }
        }
    }
    #endregion

}
