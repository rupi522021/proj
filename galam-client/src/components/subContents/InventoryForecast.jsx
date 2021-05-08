import { FormControl, InputLabel, MenuItem, Select, TextField, } from '@material-ui/core';
import React, { Component } from 'react'
import { MsgContext, apiUrl, fetchCall } from '../../consts/MainConst';
import VsbTable from '../VsbTable';

export default class InventoryForecast extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.data = [];
    this.newParams = true;
    this.savedChangeFlag = false;
    this.savedFlag = false;
    this.similationModeToFalseFlag = false;
    this.emptyChanged = {
      S: false,
      N: false,
      D: false,
      E: false,
      OF: false,
      Q1: false,
      Q2: false,
      Q3: false,
      Q4: false,
    };
    this.Changed = { ...this.emptyChanged };
    this.emptyQ = {
      Year: null,
      Quarter: null,
      DailyProduction: null,
      GreensUsing: null,
      FactorS: null,
      FactorN: null,
      FactorD: null,
      FactorE: null,
      FactorOF: null,
      FactorL: null,
      StorageCapacity: null,
      ConteinersInventory: null,
      PeriodInPast: true
    };
    this.OriginalPeriods = [];
    this.PeriodsBeforeSimulation = {};
    this.Periods = {
      Q1: { ...this.emptyQ },
      Q2: { ...this.emptyQ },
      Q3: { ...this.emptyQ },
      Q4: { ...this.emptyQ }
    }
    this.DistributionBeforeSimulation = {};
    this.OriginalDistribution = {};
    this.Distribution = { S: null, N: null, D: null, E: null, OF: null, }
    this.InventoryYears = [];
    this.columns = [
      { name: "שבוע", data: "WeekNumber", render: this.forecastRender },
      { name: "רבעון", data: "QuarterP", render: this.forecastRender },
      { name: "ייצור מתוכנן", data: "ActualProduction", render: this.forecastRender },
      { name: "שימוש בגרינס", data: "ActualGreens", render: this.forecastRender },
      { name: "מכירות מתוכננות", data: "ActualSales", render: this.forecastRender },
      { name: "מלאי מתוכנן", data: "ActualInventory", render: this.forecastRender },
      { name: "Fruitose S", data: "ActualInventoryS", render: this.forecastRender },
      { name: "Fruitose N", data: "ActualInventoryN", render: this.forecastRender },
      { name: "Fruitose D", data: "ActualInventoryD", render: this.forecastRender },
      { name: "Fruitose E", data: "ActualInventoryE", render: this.forecastRender },
      { name: "Fruitose OF", data: "ActualInventoryOF", render: this.forecastRender },
      { name: "מלאי מתוכנן באחסנה חיצונית", data: "ActualInventoryExternal", render: this.forecastRender },
    ];
    this.savedBeforeSimulation = true;
    this.state = {
      yearValue: "",
      saved: true,
      similationMode: false,
    }
    this.classes = {
      yearStyle: {
        margin: 5,
        width: 100,
        direction: 'ltr'
      }
    };
  }

  forecastRender = (row, data, index) => { return (<div className={`${row.IsActual ? "greyBg" : row[data] < 0 ? "invMinus" : ""}`}>{row[data]}</div>); }

  componentDidMount() { this.getInventoryYears(); }

  getInventoryYears = () => {
    fetchCall('GET', `${apiUrl}/InventoryYears/`, '', this.getInventoryYearsSuccess, this.getInventoryYearsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getInventoryYearsSuccess = (data) => {
    this.InventoryYears = data.Years;
    this.getInventoryParams(data.CurrentYear);
  }

  getInventoryYearsError = (error) => { this.MessageShow(error.Message, 0); }

  getInventoryParams = (year) => {
    fetchCall('GET', `${apiUrl}/InventoryParams/${year = null ? "" : `?year=${year}`}`, '',
      this.getInventoryParamsSuccess, this.getInventoryParamsError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getInventoryParamsSuccess = (data) => {
    this.OriginalPeriods = data.Periods;
    this.Periods = {};
    this.OriginalPeriods.forEach(item => { this.Periods[`Q${item.QuarterP}`] = { ...item } });
    this.OriginalDistribution = data.Distribution;
    this.Distribution = { ...this.OriginalDistribution };
    this.newParams = true;
    this.getInventoryForecast(data.Year);
  }

  getInventoryParamsError = (error) => { this.MessageShow(error.Message, 0); }

  getInventoryForecast = (year) => {
    fetchCall('GET', `${apiUrl}/InventoryForecast/${year = null ? "" : `?year=${year}`}`, '',
      this.getInventoryForecastSuccess, this.getInventoryForecastError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  getInventoryForecastSuccess = (data) => {
    this.data = data.InventoryForecast;
    if (this.newParams) {
      this.Changed = { ...this.emptyChanged };
      if (this.state.similationMode) {
        this.PeriodsBeforeSimulation = {
          Q1: { ...this.Periods.Q1 },
          Q2: { ...this.Periods.Q2 },
          Q3: { ...this.Periods.Q3 },
          Q4: { ...this.Periods.Q4 }
        };
        this.DistributionBeforeSimulation = { ...this.Distribution };
        this.savedBeforeSimulation = true;
      }
      this.newParams = false;
      this.setState({ yearValue: data.Year, saved: true });
    }
    let state = {};
    state["yearValue"] = data.Year;
    if (this.savedChangeFlag) {
      this.savedChangeFlag = false;
      state["saved"] = this.savedFlag;
    }
    if (this.similationModeToFalseFlag) {
      this.similationModeToFalseFlag = false;
      state["similationMode"] = false;
    }
    this.setState(state);
  }

  getInventoryForecastError = (error) => { this.MessageShow(error.Message, 0); }

  getInventoryYearsError = (error) => { this.MessageShow(error.Message, 0); }

  yearChange = (e) => { this.getInventoryParams(e.target.value); }

  refresh = () => {
    this.similationModeToFalseFlag = true;
    this.getInventoryParams(this.state.yearValue);
  }

  save = () => {
    if (Number(this.Distribution.S) + Number(this.Distribution.N) + Number(this.Distribution.D) + Number(this.Distribution.E) + Number(this.Distribution.OF) != 1)
      this.MessageShow("סכום הפילוג חייב להיות 100%", 0);
    else {
      let periodsToChange = [];
      ["Q1", "Q2", "Q3", "Q4"].forEach(item => { if (this.Changed[item]) periodsToChange.push(this.Periods[item]) });
      let disArr = [];
      ["S", "N", "D", "E", "OF"].forEach(item => { if (this.Changed[item]) disArr.push(`${item}=${this.Distribution[item]}`) });
      let disStr = "";
      if (disArr.length != 0) disStr = `?${disArr.join("&")}`;
      fetchCall('POST', `${apiUrl}/InventoryParams/${disStr}`, periodsToChange, this.saveSuccess, this.saveError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
    }
  }

  saveSuccess = (data) => {
    this.Changed = { ...this.emptyChanged };
    if (this.state.similationMode) {
      this.PeriodsBeforeSimulation = {
        Q1: { ...this.Periods.Q1 },
        Q2: { ...this.Periods.Q2 },
        Q3: { ...this.Periods.Q3 },
        Q4: { ...this.Periods.Q4 }
      };
      this.DistributionBeforeSimulation = { ...this.Distribution };
      this.savedBeforeSimulation = true;
    }
    this.MessageShow(data.Message, 1);
    this.setState({ saved: true });
  }

  saveError = (error) => { this.MessageShow(error.Message, 0); }

  menuTDRender = (obj, firstF, secondF = null, isPercent = false) => {
    let tmp = secondF == null ? this[obj][firstF] : this[obj][firstF][secondF];
    let res = tmp == null || tmp == "" ? "" : (isPercent ? Math.round(tmp * 100) : tmp);
    let disabled = true;
    if (this.props.logedInUser.Permissions.includes(5) || this.state.similationMode) {
      if (secondF == null) disabled = false;
      else if (!this[obj][firstF].PeriodInPast) disabled = false;
    }
    return (
      <td key={`${obj}${firstF}${secondF == null ? "" : secondF}`}>
        <TextField
          variant="outlined"
          size="small"
          value={res}
          disabled={disabled}
          key={`${obj}${firstF}${secondF == null ? "" : secondF}input`}
          onChange={this.inputChange}
          style={this.classes.inputStyle}
          inputProps={{ "data-obj": obj, "data-firstf": firstF, "data-secondf": secondF == null ? "" : secondF }}
        />
      </td>
    );
  }

  inputChange = (e) => {
    if (/^\d+$/.test(e.currentTarget.value) || e.currentTarget.value == "") {
      if (e.currentTarget.dataset.secondf == "") {
        this[e.currentTarget.dataset.obj][e.currentTarget.dataset.firstf] = e.currentTarget.value / 100;
        this.Changed[e.currentTarget.dataset.firstf] = true;
      }
      else {
        this[e.currentTarget.dataset.obj][e.currentTarget.dataset.firstf][e.currentTarget.dataset.secondf] = e.currentTarget.value;
        this.Changed[e.currentTarget.dataset.firstf] = true;
      }
      if (this.props.logedInUser.Permissions.includes(5)) {
        this.savedChangeFlag = true;
        this.savedFlag = false;
      }
      this.simulationInventory();
    }
    else this.setState({});
  }

  simulationInventory = () => {
    let periods = ["Q1", "Q2", "Q3", "Q4"].map(item => this.Periods[item]);
    let api = `${apiUrl}/SimulationInventory/?${["S", "N", "D", "E", "OF"].map(item => `${item}=${this.Distribution[item]}`).join("&")}&year=${this.state.yearValue}`;
    fetchCall('POST', api, periods, this.getInventoryForecastSuccess, this.getInventoryForecastError, this.props.logedInUser.Token, this.props.logedInUser.UserName);
  }

  avg = (obj, firstFArr, secondF) => {
    let allNull = true;
    let tmp = 0;
    firstFArr.forEach(item => {
      if (this[obj][item][secondF] != null && this[obj][item][secondF] != "") {
        allNull = false;
        tmp = tmp + Number(this[obj][item][secondF]);
      }
    });
    return (<td>{firstFArr.length == 0 || allNull || isNaN(tmp) ? null : Math.round((100 * tmp / firstFArr.length)) / 100}</td>);
  }

  total = (obj, firstFArr, secondF) => {
    let allNull = true;
    let tmp = 0;
    firstFArr.forEach(item => {
      if (this[obj][item][secondF] != null && this[obj][item][secondF] != "") {
        allNull = false;
        tmp = tmp + Number(this[obj][item][secondF]);
      }
    });
    return (<td>{firstFArr.length == 0 || allNull ? null : tmp}</td>);
  }

  onClickSimulation = () => {
    this.PeriodsBeforeSimulation = {
      Q1: { ...this.Periods.Q1 },
      Q2: { ...this.Periods.Q2 },
      Q3: { ...this.Periods.Q3 },
      Q4: { ...this.Periods.Q4 }
    };
    this.DistributionBeforeSimulation = { ...this.Distribution };
    this.savedBeforeSimulation = this.state.saved;
    this.setState({ similationMode: true });
  }

  onClickCancelSimulation = () => {
    this.Periods = this.PeriodsBeforeSimulation;
    this.Distribution = this.DistributionBeforeSimulation;
    this.similationModeToFalseFlag = true;
    this.simulationInventory();
  }

  render() {
    return (
      <div>
        <div className="divToolbar">
          <div></div>
          <div className="divToolbarImg">
            {this.state.similationMode ? [
              <img src="img/Delete.png" onClick={this.onClickCancelSimulation} />,
              <span className="green">מתבצעת סימולציה</span>
            ] : ""}
            <img src="img/Refresh.png" onClick={this.refresh} />
            {this.state.saved ?
              <img className="disabled" src="img/Save_disabled.png" /> :
              <img src="img/Save_normal.png" onClick={this.save} />}
          </div>
          <div>
          </div>
        </div>
        <div className="invDiv">
          <VsbTable
            sortable={false}
            data={this.data}
            columns={this.columns}
            divClass="heightWithTwoMenu">
            <FormControl variant="outlined" margin="dense" style={this.classes.yearStyle}>
              <InputLabel id="yearSelect-label">שנה</InputLabel>
              <Select labelId="yearSelect-label" id="yearSelect" value={this.state.yearValue} onChange={this.yearChange} label="שנה">
                {this.InventoryYears.map(item => <MenuItem key={item} value={item}>{item}</MenuItem>)}
              </Select>
            </FormControl>
          </VsbTable>
          <div className="divFM">
            <table className="menuTb">
              <thead>
                <tr>
                  <th>ייצור תקופתי</th>
                  <th>Q1</th>
                  <th>Q2</th>
                  <th>Q3</th>
                  <th>Q4</th>
                  <th>AVG</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td className="tdHRow">ייצור יומי מתוכנן</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "DailyProduction"))}
                  {this.avg("Periods", ["Q1", "Q2", "Q3", "Q4"], "DailyProduction")}
                </tr>
                <tr>
                  <td className="tdHRow">שימוש שבועי בגרינס</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "GreensUsing"))}
                  {this.avg("Periods", ["Q1", "Q2", "Q3", "Q4"], "GreensUsing")}
                </tr>
              </tbody>
            </table>
            <table className="menuTb">
              <thead>
                <tr>
                  <th>פרקציה</th>
                  <th>S %</th>
                  <th>N %</th>
                  <th>D %</th>
                  <th>E %</th>
                  <th>OF %</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td className="tdHRow">פילוג</td>
                  {["S", "N", "D", "E", "OF"].map(item => this.menuTDRender("Distribution", item, null, true))}
                </tr>
              </tbody>
            </table>
            <table className="menuTb">
              <thead>
                <tr>
                  <th>פקטור מכירות</th>
                  <th>Q1</th>
                  <th>Q2</th>
                  <th>Q3</th>
                  <th>Q4</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td className="tdHRow">Frutose S</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "FactorS"))}
                  {this.total("Periods", ["Q1", "Q2", "Q3", "Q4"], "FactorS")}
                </tr>
                <tr>
                  <td className="tdHRow">Frutose N</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "FactorN"))}
                  {this.total("Periods", ["Q1", "Q2", "Q3", "Q4"], "FactorN")}
                </tr>
                <tr>
                  <td className="tdHRow">Frutose D</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "FactorD"))}
                  {this.total("Periods", ["Q1", "Q2", "Q3", "Q4"], "FactorD")}
                </tr>
                <tr>
                  <td className="tdHRow">Frutose E</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "FactorE"))}
                  {this.total("Periods", ["Q1", "Q2", "Q3", "Q4"], "FactorE")}
                </tr>
                <tr>
                  <td className="tdHRow">Frutose OF</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "FactorOF"))}
                  {this.total("Periods", ["Q1", "Q2", "Q3", "Q4"], "FactorOF")}
                </tr>
                <tr>
                  <td className="tdHRow">Liquid</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "FactorL"))}
                  {this.total("Periods", ["Q1", "Q2", "Q3", "Q4"], "FactorL")}
                </tr>
              </tbody>
            </table>
            <table className="menuTb">
              <thead>
                <tr>
                  <th>אחסנה</th>
                  <th>Q1</th>
                  <th>Q2</th>
                  <th>Q3</th>
                  <th>Q4</th>
                  <th>AVG</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td className="tdHRow">יכולת אחסנה בגלעם</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "StorageCapacity"))}
                  {this.avg("Periods", ["Q1", "Q2", "Q3", "Q4"], "StorageCapacity")}
                </tr>
                <tr>
                  <td className="tdHRow">מלאי מתוכנן במכולות</td>
                  {["Q1", "Q2", "Q3", "Q4"].map(item => this.menuTDRender("Periods", item, "ConteinersInventory"))}
                  {this.avg("Periods", ["Q1", "Q2", "Q3", "Q4"], "ConteinersInventory")}
                </tr>
              </tbody>
            </table>
            {this.state.similationMode ? "" :
              <div className="cntTxt">
                <button className="blueButton simulationButton" onClick={this.onClickSimulation}>ביצוע סימולציה</button>
              </div>}
          </div>
        </div>
      </div>
    )
  }
}