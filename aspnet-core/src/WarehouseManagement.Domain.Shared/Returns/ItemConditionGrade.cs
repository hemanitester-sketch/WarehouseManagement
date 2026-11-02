namespace WarehouseManagement.Returns;

public enum ItemConditionGrade
{
    GradeA_BrandNew = 1,     // Factory sealed, undamaged, 100% restockable
    GradeB_OpenBox = 2,      // Open box, minor cosmetic packaging wear, restockable
    GradeC_Defective = 3,    // Faulty or damaged, needs vendor return or repair
    GradeD_Scrap = 4         // Complete loss, write-off / scrap disposal
}
