import { FormControl, InputLabel, MenuItem, Select, } from '@material-ui/core';
import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall } from '../../consts/MainConst';
import VsbTable from '../VsbTable';

export default class StatisticalForecast extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.StatisticalForecastYears = [];
    this.data = [];
    this.columns = [
      { name: "Sale manager", data: "PersonFullName" },
      { name: "Customer number", data: "CustomerNumber" },
      { name: "Customer name", data: "CustomerName" },
      { name: "Product family", data: "ProductFamilyName" },
      { name: "Q1", data: "Q1" },
      { name: "Q2", data: "Q2" },
      { name: "Q3", data: "Q3" },
      { name: "Q4", data: "Q4" },
      { name: "Total", data: "Total" },
    ];
    this.state = {
      yearValue: ""
    };
    this.classes = {
      yearStyle: {
        margin: 5,
        width: 100,
        direction: 'ltr'
      }
    };
    this.classes = {
      yearStyle: {
        margin: 5,
        width: 100,
        direction: 'ltr'
      }
    };
  }

  componentDidMount() { this.getStatisticalForecastYears(); }

  getStatisticalForecastYears = () => {
    fetchCall('GET', `${apiUrl}/StatisticalForecastYears/`, '', this.getStatisticalForecastYearsSuccess,
      this.getStatisticalForecastYearsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getStatisticalForecastYearsSuccess = (data) => {
    this.StatisticalForecastYears = data.Years;
    this.getStatisticalForecast(data.Years[0]);
  }

  getStatisticalForecastYearsError = (error) => { this.MessageShow(error.Message, 0); }

  getStatisticalForecast = (year) => {
    let api = `${apiUrl}/StatisticalForecast/${year == undefined ? "" : `?year=${year}`}`
    fetchCall('GET', api, '', this.getStatisticalForecastSuccess, this.getStatisticalForecastError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getStatisticalForecastSuccess = (data) => {
    this.data = data.StatisticalForecast;
    this.setState({ yearValue: data.Year });
  }

  getStatisticalForecastError = (error) => {
    if (error.Code == "NotFound") {
      this.data = [];
      this.setState({ yearValue: error.Year });
    }
    this.MessageShow(error.Message, 0);
  }

  yearChange = (e) => { this.getStatisticalForecast(e.target.value); }

  refresh = () => { 
    let api = `${apiUrl}/StatisticalForecast/${this.state.yearValue == undefined ? "" : `?year=${this.state.yearValue}`}`
    fetchCall('GET', api, '', this.refreshSuccess, this.getStatisticalForecastError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
   }

  refreshSuccess = (data) => {
    this.MessageShow("בוצע ריענון", 1);
    this.getStatisticalForecastSuccess(data);
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
              {this.StatisticalForecastYears.map(item => <MenuItem key={item} value={item}>{item}</MenuItem>)}
            </Select>
          </FormControl>
        </VsbTable>
      </div>
    )
  }
}