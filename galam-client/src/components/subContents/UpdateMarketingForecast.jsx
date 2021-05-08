import { FormControl, InputLabel, MenuItem, Select, TextField, Tooltip, } from '@material-ui/core';
import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall } from '../../consts/MainConst';
import AddForecastRow from '../Modals/AddForecastRow';
import VsbTable from '../VsbTable';

export default class UpdateMarketingForecast extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.saved = true;
    this.saveIconRef = React.createRef();
    this.data = [];
    this.ForecastUpdateYears = [];
    this.lockedQuarters = [];
    this.inputs = {};
    this.columns = [
      { name: "Sale manager", data: "PersonFullName" },
      { name: "Region", data: "Market" },
      { name: "Country", data: "CountryName" },
      { name: "Customer number", data: "CustomerNumber" },
      { name: "Customer name", data: "CustomerName" },
      { name: "End customer", data: "ShipToName" },
      { name: "Item", data: "ItemNumber" },
      { name: "Item description", data: "ItemDescription" },
      { name: "Product family", data: "ProductFamilyName" },
      { name: "Melting/Classic", data: "MeltingClassic" },
      { name: "Q1", data: "Q1", render: this.inputRender },
      { name: "Q2", data: "Q2", render: this.inputRender },
      { name: "Q3", data: "Q3", render: this.inputRender },
      { name: "Q4", data: "Q4", render: this.inputRender },
    ];
    this.allItems = [];
    this.allShipTos = [];
    this.state = {
      modalToShow: "",
      yearValue: ""
    }
    this.classes = {
      yearStyle: {
        margin: 5,
        width: 100,
        direction: 'ltr'
      },
      inputStyle: {
        width: 100,
      }
    };
  }

  inputRender = (row, data, index) => {
    if (this.lockedQuarters.includes(Number(data[1]))) return (
      <Tooltip title={`${data} חסום לעדכון`}>
        <TextField
          type="number"
          variant="outlined"
          size="small"
          defaultValue={this.data[index][data]}
          disabled
          key={`${data}${row.StringKey}`}
          onChange={this.inputChange}
          style={this.classes.inputStyle}
          inputProps={{ "data-rowkey": row.StringKey, "data-feild": data, "data-index": index }}
        />
      </Tooltip>);
    else {
      if (this.data[index].LockedToUser == this.props.logedInUser.UserName) return (
        <TextField
          type="number"
          variant="outlined"
          size="small"
          defaultValue={this.data[index][data]}
          key={`${data}${row.StringKey}`}
          onChange={this.inputChange}
          style={this.classes.inputStyle}
          inputProps={{ "data-rowkey": row.StringKey, "data-feild": data, "data-index": index }}
        />);
      else return (
        <Tooltip title={`משתמש ${this.data[index].LockedToUser} מעדכן שורה זאת`}>
          <TextField
            type="number"
            variant="outlined"
            size="small"
            defaultValue={this.data[index][data]}
            disabled
            key={`${data}${row.StringKey}`}
            onChange={this.inputChange}
            style={this.classes.inputStyle}
            inputProps={{ "data-rowkey": row.StringKey, "data-feild": data, "data-index": index }}
          />
        </Tooltip>);
    }
  }

  componentDidMount() { this.getForecastUpdateYears(); }

  componentWillUnmount = () => {
    fetchCall('DELETE', `${apiUrl}/ClearLockes/`, '', () => { }, () => { }, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  inputChange = (e) => {
    let tmpVal = e.currentTarget.value;
    if (tmpVal != '') if (Number(tmpVal) < 0) tmpVal = 0;
    let tmp = this.data[e.currentTarget.dataset.index];
    tmp[e.currentTarget.dataset.feild] = tmpVal;
    tmp.Total = Math.round((Number(tmp.Q1) + Number(tmp.Q2) + Number(tmp.Q3) + Number(tmp.Q4)) * 10) / 10;
    if (tmp.QuartersToUpdate == undefined) tmp.QuartersToUpdate = [];
    if (!tmp.QuartersToUpdate.includes(Number(e.currentTarget.dataset.feild[1]))) tmp.QuartersToUpdate.push(Number(e.currentTarget.dataset.feild[1]));
    this.saved = false;
    this.saveIconRef.current.className = "";
    this.saveIconRef.current.src = "img/Save_normal.png";
  }

  totalRender = (row, data, index) => {
    return (
      Math.round((Number(row.Q1) + Number(row.Q2) + Number(row.Q3) + Number(row.Q4)) * 10) / 10
    );
  }

  getForecastUpdateYears = () => {
    fetchCall('GET', `${apiUrl}/ForecastUpdateYears/`, '', this.getForecastUpdateYearsSuccess,
      this.getForecastUpdateYearsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getForecastUpdateYearsSuccess = (data) => {
    this.ForecastUpdateYears = data.Years;
    this.GetForecastToUpdate(data.Years[0]);
  }

  getForecastUpdateYearsError = (error) => { this.MessageShow(error.Message, 0); }

  GetForecastToUpdate = (year) => {
    let api = `${apiUrl}/ForecastToUpdate/${year == undefined ? "" : `?year=${year}`}`
    fetchCall('GET', api, '', this.GetForecastToUpdateSuccess, this.GetForecastToUpdateError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  GetForecastToUpdateSuccess = (data) => {
    this.data = data.RowsToUpdate;
    this.saved = true;
    this.saveIconRef.current.className = "disabled";
    this.saveIconRef.current.src = "img/Save_disabled.png";
    this.lockedQuarters = data.LockedQuarters;
    this.setState({ yearValue: data.Year });
  }

  GetForecastToUpdateError = (error) => {
    if (error.Code == "NotFound") {
      this.data = [];
      this.saved = true;
      this.saveIconRef.current.className = "disabled";
      this.saveIconRef.current.src = "img/Save_disabled.png";
      this.setState({ yearValue: error.Year });
    }
    this.MessageShow(error.Message, 0);
  }

  yearChange = (e) => {
    this.GetForecastToUpdate(e.target.value);
  }

  addForecastRow = () => {
    if (this.lockedQuarters.length < 4)
      fetchCall('GET', `${apiUrl}/Items/`, '', this.getItemsSuccess, this.getItemsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
    else this.MessageShow("לא ניתן להוסיף שורות", 0);
  }

  getItemsSuccess = (data) => {
    this.allItems = data.Items;
    fetchCall('GET', `${apiUrl}/ShipTos/`, '', this.getShipTosSuccess, this.getShipTosError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getShipTosSuccess = (data) => {
    this.allShipTos = data.ShipTos;
    this.setState({ modalToShow: "AddForecastRow" });
  }

  getShipTosError = () => {
    this.setState({ modalToShow: "AddForecastRow" });
  }

  getItemsError = (error) => { this.MessageShow(error.Message, 0); }

  hideModal = () => { this.setState({ modalToShow: "" }); }

  addForecastRowSave = (data) => {
    console.log(data);
    fetchCall('PUT', `${apiUrl}/Forecast/`, data, this.addForecastRowSaveSuccess, this.addForecastRowSaveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  addForecastRowSaveSuccess = (data) => {
    this.MessageShow(data.Message, 1, 10000);
    if (data.UpdateStatus != "Pending") this.data.push(data.ForecastRow);
    this.setState({ modalToShow: "" });
  }

  addForecastRowSaveError = (error) => { this.MessageShow(error.Message, 0); }

  save = () => {
    if (!this.saved) fetchCall('POST', `${apiUrl}/Forecast/`, this.data, this.saveSuccess, this.saveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  saveSuccess = (data) => {
    this.saved = true;
    this.saveIconRef.current.className = "disabled";
    this.saveIconRef.current.src = "img/Save_disabled.png";
    this.MessageShow(data.Message, 1);
  }

  saveError = (error) => { this.MessageShow(error.Message, 0); }

  render() {
    return (
      <div>
        <div className="divToolbar">
          <div></div>
          <div className="divToolbarImg">
            <img src="img/plus.png" onClick={this.addForecastRow} />
            {this.saved ?
              <img ref={this.saveIconRef} className="disabled" src="img/Save_disabled.png" onClick={this.save} /> :
              <img ref={this.saveIconRef} src="img/Save_normal.png" onClick={this.save} />}
          </div>
          <div></div>
        </div>
        <VsbTable
          sortable={true}
          data={this.data}
          columns={this.columns}
          divClass="heightWithTwoMenu"
          defaultSortBy="Customer name">
          <FormControl variant="outlined" margin="dense" style={this.classes.yearStyle}>
            <InputLabel id="yearSelect-label">שנה</InputLabel>
            <Select labelId="yearSelect-label" id="yearSelect" value={this.state.yearValue} onChange={this.yearChange} label="שנה">
              {this.ForecastUpdateYears.map(item => <MenuItem key={item} value={item}>{item}</MenuItem>)}
            </Select>
          </FormControl>
        </VsbTable>
        {this.state.modalToShow == "AddForecastRow" ?
          <AddForecastRow
            Year={this.state.yearValue}
            LockedQuarters={this.lockedQuarters}
            AllShipTos={this.allShipTos}
            AllItems={this.allItems}
            saveF={this.addForecastRowSave}
            cancelX={this.hideModal}
            logedInUser={this.props.logedInUser} /> :
          ""}
      </div>
    )
  }
}