import { FormControl, InputLabel, OutlinedInput, TextField } from '@material-ui/core';
import React, { Component } from 'react'
import { MsgContext } from '../../consts/MainConst';
import ModalWidow from './ModalWidow'
import DateFnsUtils from '@date-io/date-fns';
import { MuiPickersUtilsProvider, KeyboardDateTimePicker } from '@material-ui/pickers';
import Autocomplete from '@material-ui/lab/Autocomplete';

export default class FindTransactionsMod extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.quarters = [1, 2, 3, 4];
    this.ShipTos = [];
    this.AllMarkets = [];
    for (let i = 0; i < this.props.AllCountries.length; i++)
      if (!this.AllMarkets.includes(this.props.AllCountries[i].CountryMarket)) this.AllMarkets.push(this.props.AllCountries[i].CountryMarket);
    this.state = {
      from: null,
      to: null,
      year: null,
      quarter: null,
      customerNumber: "",
      shipTo: null,
      item: null,
      productFamily: null,
      market: null,
      country: null,
    }
    this.classes = {
      dateTimeStyle: {
        margin: 10,
        width: 270,
        direction: 'ltr'
      },
      inputStyle: {
        margin: 10,
        width: 270,
        direction: 'ltr',
        display: 'inline-flex',
      },
      input18Style: {
        margin: 10,
        marginTop: 18,
        width: 270,
        direction: 'ltr',
        display: 'inline-flex',
      }
    };
  }

  fromChange = (e) => { this.setState({ from: e }); }
  toChange = (e) => { this.setState({ to: e }); }
  yearChange = (e, newVal) => { this.setState({ year: newVal }); }
  quarterChange = (e, newVal) => { this.setState({ quarter: newVal }); }
  shipToChange = (e, newVal) => { this.setState({ shipTo: newVal }); }
  itemChange = (e, newVal) => { this.setState({ item: newVal }); }
  productFamilyChange = (e, newVal) => { this.setState({ productFamily: newVal }); }
  marketChange = (e, newVal) => { this.setState({ market: newVal }); }
  countryChange = (e, newVal) => { this.setState({ country: newVal }); }

  customerNumberChange = (e) => {
    this.ShipTos = [];
    for (let i = 0; i < this.props.AllShipTos.length; i++) if (this.props.AllShipTos[i].CustomerNumber == e.target.value) this.ShipTos.push(this.props.AllShipTos[i]);
    this.setState({ customerNumber: e.target.value, shipTo: null });
  }

  validateForm = () => {
    let valid = true;
    if (this.state.from == "Invalid Date" || this.state.to == "Invalid Date") {
      valid = false;
      this.MessageShow("תאריך לא תקין", 0);
    }
    return valid;
  }

  onClickFind = () => {
    if (this.validateForm()) this.props.findF(this.state);
  }

  render() {
    return (
      <div>
        <ModalWidow cancelX={this.props.cancelX} header={`חיפוש תנועות`}>
          <MuiPickersUtilsProvider utils={DateFnsUtils}>
            <KeyboardDateTimePicker
              clearable
              showTodayButton
              style={this.classes.dateTimeStyle}
              inputVariant="outlined"
              margin="dense"
              ampm={false}
              label="עד תאריך"
              value={this.state.to}
              onChange={this.toChange}
              format="dd/MM/yyyy HH:mm"
            />
          </MuiPickersUtilsProvider>
          <MuiPickersUtilsProvider utils={DateFnsUtils}>
            <KeyboardDateTimePicker
              clearable
              showTodayButton
              disableFuture
              style={this.classes.dateTimeStyle}
              inputVariant="outlined"
              margin="dense"
              ampm={false}
              label="מתאריך"
              value={this.state.from}
              onChange={this.fromChange}
              format="dd/MM/yyyy HH:mm"
            />
          </MuiPickersUtilsProvider>
          <br />
          <Autocomplete
            id="year"
            onChange={this.yearChange}
            options={this.props.Years}
            getOptionLabel={(option) => option.toString()}
            style={this.classes.inputStyle}
            renderInput={(params) => <TextField {...params} label="שנה" variant="outlined" margin="dense" />}
          />
          <Autocomplete
            id="quarter"
            onChange={this.quarterChange}
            options={this.quarters}
            getOptionLabel={(option) => option.toString()}
            style={this.classes.inputStyle}
            renderInput={(params) => <TextField {...params} label="רבעון" variant="outlined" margin="dense" />}
          />
          <br />
          <FormControl variant="outlined" margin="dense" style={this.classes.input18Style}>
            <InputLabel htmlFor="customer-number">מספר לקוח</InputLabel>
            <OutlinedInput id="customer-number" value={this.state.customerNumber} onChange={this.customerNumberChange} label="מספר לקוח" type="number" inputProps={{ "min": "0" }} />
          </FormControl>
          <Autocomplete
            id="comboShipTo"
            value={this.state.shipTo}
            onChange={this.shipToChange}
            options={this.ShipTos}
            getOptionLabel={(option) => option.ShipToName}
            style={this.classes.inputStyle}
            renderInput={(params) => <TextField {...params} label='לקוח קצה' variant="outlined" margin="dense" />}
          />
          <br />
          <Autocomplete
            id="comboItem"
            onChange={this.itemChange}
            options={this.props.AllItems}
            getOptionLabel={(option) => option.Number}
            style={this.classes.inputStyle}
            renderInput={(params) => <TextField {...params} label='מק"ט' variant="outlined" margin="dense" />}
          />
          <Autocomplete
            id="comboProductFamily"
            onChange={this.productFamilyChange}
            options={this.props.AllProductFamilies}
            getOptionLabel={(option) => option.ProductFamilyName}
            style={this.classes.inputStyle}
            renderInput={(params) => <TextField {...params} label='משפחה' variant="outlined" margin="dense" />}
          />
          <br />
          <Autocomplete
            id="comboMarket"
            onChange={this.marketChange}
            options={this.AllMarkets}
            getOptionLabel={(option) => option}
            style={this.classes.inputStyle}
            renderInput={(params) => <TextField {...params} label='שוק' variant="outlined" margin="dense" />}
          />
          <Autocomplete
            id="comboCountry"
            onChange={this.countryChange}
            options={this.props.AllCountries}
            getOptionLabel={(option) => option.CountryName}
            style={this.classes.inputStyle}
            renderInput={(params) => <TextField {...params} label="מדינה" variant="outlined" margin="dense" />}
          />
          <div className="cntTxt">
            <button className="blueButton" onClick={this.onClickFind}>חפש</button>
            <button className="blueButton" onClick={this.props.cancelX}>בטל</button>
          </div>
        </ModalWidow>
      </div>
    )
  }
}