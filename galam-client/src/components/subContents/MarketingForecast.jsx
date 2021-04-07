import { FormControl, InputLabel, MenuItem, Select, } from '@material-ui/core';
import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall } from '../../consts/MainConst';
import VsbTable from '../VsbTable';
import DateFnsUtils from '@date-io/date-fns';
import { MuiPickersUtilsProvider, KeyboardDateTimePicker } from '@material-ui/pickers';

export default class MarketingForecast extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.data = [];
    this.ForecastYears = [];
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
      { name: "Q1", data: "Q1" },
      { name: "Q2", data: "Q2" },
      { name: "Q3", data: "Q3" },
      { name: "Q4", data: "Q4" },
      { name: "Total", data: "Total" },
    ];
    this.state = {
      yearValue: "",
      selectedDate: null,
    }
    this.classes = {
      dateTimeStyle: {
        margin: 5,
        width: 270,
        direction: 'ltr'
      },
      yearStyle: {
        margin: 5,
        width: 100,
        direction: 'ltr'
      }
    };
  }

  componentDidMount() { this.getForecastYears(); }

  getForecastYears = () => {
    fetchCall('GET', `${apiUrl}/ForecastYears/`, '', this.getForecastYearsSuccess, this.getForecastYearsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getForecastYearsSuccess = (data) => {
    this.ForecastYears = data.Years;
    this.getForecast();
  }

  getForecastYearsError = (error) => { this.MessageShow(error.Message, 0); }

  getForecast = (year) => {
    let api = `${apiUrl}/Forecast/${year == undefined ? "" : `?year=${year}`}`
    fetchCall('GET', api, '', this.getForecastSuccess, this.getForecastError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getForecastSuccess = (data) => {
    this.data = data.Forecast;
    this.setState({ yearValue: data.Year, selectedDate: data.FDate });
  }

  getForecastError = (error) => {
    if (error.Code == "NotFound" && error.FDate != null) {
      this.data = [];
      this.setState({ yearValue: error.Year, selectedDate: error.FDate });
    }
    this.MessageShow(error.Message, 0);
  }

  yearChange = (e) => { this.getForecast(e.target.value); }

  refresh = () => {
    let tmp = new Date(this.state.selectedDate);
    let br;
    let br2 = [];
    let a;
    if (tmp != null && tmp != "Invalid Date") {
      br = tmp.toString().split(" ");
      for (let i = 0; i <= 5; i++) { br2.push(br[i]); }
      br2[5] = "GMT+0000";
      a = new Date(br2.join(" "));
    }
    let api = `${apiUrl}/Forecast/?year=${this.state.yearValue}${(tmp == null || tmp == "Invalid Date" ? "" : `&fDate=${a.toISOString().split(".")[0]}`)}`;
    fetchCall('GET', api, '', this.refreshSuccess, this.getForecastError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  refreshSuccess = (data) => {
    this.MessageShow("בוצע ריענון", 1);
    this.getForecastSuccess(data);
  }

  dateTimePickerChange = (e) => {
    if (e == "Invalid Date") this.setState({ selectedDate: e });
    else {
      let br;
      let br2 = [];
      let a;
      if (e != null) {
        br = e.toString().split(" ");
        for (let i = 0; i <= 5; i++) { br2.push(br[i]); }
        br2[5] = "GMT+0000";
        a = new Date(br2.join(" "));
      }
      let api = `${apiUrl}/Forecast/?year=${this.state.yearValue}${e == null ? "" : `&fDate=${a.toISOString().split(".")[0]}`}`;
      fetchCall('GET', api, '', this.getForecastSuccess, this.getForecastError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
    }
  }

  render() {
    return (
      <div>
        <div className="divToolbar">
          <div></div>
          <div className="divToolbarImg">
            <img src="img/Refresh.png" onClick={this.refresh} />
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
              {this.ForecastYears.map(item => <MenuItem key={item} value={item}>{item}</MenuItem>)}
            </Select>
          </FormControl>
          <MuiPickersUtilsProvider utils={DateFnsUtils}>
            <KeyboardDateTimePicker
              clearable
              showTodayButton
              style={this.classes.dateTimeStyle}
              inputVariant="outlined"
              margin="dense"
              ampm={false}
              label="תחזית לתאריך"
              value={this.state.selectedDate}
              onChange={this.dateTimePickerChange}
              format="dd/MM/yyyy HH:mm"
            />
          </MuiPickersUtilsProvider>
        </VsbTable>
      </div>
    )
  }
}