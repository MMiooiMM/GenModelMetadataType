# GenModelMetadataType

GenModelMetadataType 針對 Entity Framework Tool 產生的 Model 類別進行擴充，將自動產生帶有 [ModelMetadataType] 屬性的 Partial Class。

## 使用方法

### `genmodelmetadatatype`

顯示可用的工具列表。

### `genmodelmetadatatype list [Options]`

顯示專案檔內所有包含 **DbContext** 的類別。

 |選項|縮寫|說明|
 |:---|:--|:---|
 |--project \<PROJECT\>|-p|目標專案之專案資料夾的相對路徑。 預設值為目前的資料夾。|
 
### `genmodelmetadatatype generate [Options]`

產生帶有 [ModelMetadataType] 屬性的 Partial Class。
 
|選項|縮寫|說明|
|:---|:--|:---|
|--project \<PROJECT\>|-p|目標專案之專案資料夾的相對路徑。 預設值為目前的資料夾。|
|--context \<NAME\>|-c|目標 DbContext 的類別名稱。 預設值為專案檔中的第一筆。|
|--output-dir \<PATH\>|-o|要放置實體類別檔案的相對路徑。 預設值為目前的資料夾。|
|--force|-f|覆寫既有檔案。|
|--verbose|-v|輸出執行時的資訊。|
