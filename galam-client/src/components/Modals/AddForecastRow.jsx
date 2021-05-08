import React, { Component } from 'react'
import { FormControl, InputLabel, MenuItem, OutlinedInput, Select, TextField } from '@material-ui/core';
import Autocomplete from '@material-ui/lab/Autocomplete';
import ModalWidow from './ModalWidow';
import { MsgContext } from '../../consts/MainConst';

export default class AddForecastRow extends Component {
  static contextType = MsgContext;
  MessageShow = (msg, isSuccessShow, delMilSec = 3000) => { this.context(msg, isSuccessShow, delMilSec = 3000) };
  constructor(props) {
    super(props);
    this.ShipTos = [];
    this.state = {
      customerNumber: '',
      item: null,
      ShipTo: null,
      MCValue: "Classic",
      Q1: '',
      Q2: '',
      Q3: '',
      Q4: '',
    }
    this.classes = {
      inputStyle: {
        margin: 10,
        width: 250,
        direction: 'ltr',
        display: "inline-flex"
      },
      qStyle: {
        margin: 10,
        width: 100,
        direction: 'ltr',
        display: "inline-flex"
      }
    };
  }

  itemChange = (e, newVal) => {
    this.setState({ item: newVal });
  }

  shipToChange = (e, newVal) => {
    this.setState({ ShipTo: newVal });
  }

  customerNumberChange = (e) => {
    this.ShipTos = [];
    for (let i = 0; i < this.props.AllShipTos.length; i++) if (this.props.AllShipTos[i].CustomerNumber == e.target.value) this.ShipTos.push(this.props.AllShipTos[i]);
    this.setState({ customerNumber: e.target.value, ShipTo: null });
  }

  qChange = (e) => {
    this.setState(() => {
      let tmp = {};
      tmp[`Q${e.target.dataset.q}`] = e.target.value;
      return tmp;
    });
  }

  MCValueChange = (e) => { this.setState({ MCValue: e.target.value }); }

  validateForm = () => {
    let valid = true;
    if (this.state.customerNumber == "") {
      valid = false;
      this.MessageShow("נדרש להזין מספר לקוח", 0);
    }
    if (this.state.item == null) {
      valid = false;
      this.MessageShow('נדרש להזין מק"ט', 0);
    }
    for (let i = 1; i <= 4; i++) {
      if (this.state[`Q${i}`] == "") {
        valid = false;
        this.MessageShow(`נדרש להזין ערך בQ${i}`, 0);
      }
      else if (this.state[`Q${i}`] < 0) {
        valid = false;
        this.MessageShow(`ערך לא תקין בQ${i}`, 0);
      }
    }
    return valid;
  }

  onClickSave = () => {
    if (this.validateForm()) this.props.saveF({
      ItemNumber: this.state.item.Number,
      Year: this.props.Year,
      CustomerNumber: this.state.customerNumber,
      ShipToName: (this.state.ShipTo == null ? "" : this.state.ShipTo.ShipToName),
      MeltingClassic: this.state.MCValue,
      Q1: (this.state.Q1 == "" ? 0 : this.state.Q1),
      Q2: (this.state.Q2 == "" ? 0 : this.state.Q2),
      Q3: (this.state.Q3 == "" ? 0 : this.state.Q3),
      Q4: (this.state.Q4 == "" ? 0 : this.state.Q4),
      QuartersToUpdate: [1, 2, 3, 4]
    });
  }

  render() {
    return (
      <ModalWidow cancelX={this.props.cancelX} header={`שורת תחזית חדשה`}>
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel htmlFor="customer-number">מספר לקוח</InputLabel>
          <OutlinedInput id="customer-number" value={this.state.customerNumber} onChange={this.customerNumberChange} label="מספר לקוח" type="number" inputProps={{ "min": "0" }} />
        </FormControl>
        <br />
        <Autocomplete
          id="comboShipTo"
          value={this.state.ShipTo}
          onChange={this.shipToChange}
          options={this.ShipTos}
          getOptionLabel={(option) => option.ShipToName}
          style={this.classes.inputStyle}
          renderInput={(params) => <TextField {...params} label='שם לקוח קצה' variant="outlined" />}
        />
        <br />
        <Autocomplete
          id="comboItem"
          onChange={this.itemChange}
          options={this.props.AllItems}
          getOptionLabel={(option) => option.Number}
          style={this.classes.inputStyle}
          renderInput={(params) => <TextField {...params} label='מק"ט' variant="outlined" />}
        />
        <br />
        <FormControl variant="outlined" style={this.classes.inputStyle}>
          <InputLabel id="MCSelect-label">Melting/Classic</InputLabel>
          <Select labelId="MCSelect-label" id="MCSelect" value={this.state.MCValue} onChange={this.MCValueChange} label="Melting/Classic">
            <MenuItem key="Classic" value="Classic">Classic</MenuItem>
            <MenuItem key="Melting" value="Melting">Melting</MenuItem>
          </Select>
        </FormControl>
        <br />
        <div style={{ direction: 'ltr' }}>
          {[1, 2, 3, 4].map(i =>
            <TextField
              disabled={this.props.LockedQuarters.includes(i)}
              key={`Q${i}`}
              label={`Q${i}`}
              variant="outlined"
              value={this.state[`Q${i}`]}
              style={this.classes.qStyle}
              type="number"
              onChange={this.qChange}
              inputProps={{ "min": "0", "data-q": i }} />)}
        </div>
        <div className="cntTxt">
          <button className="blueButton" onClick={this.onClickSave}>שמור</button>
          <button className="blueButton" onClick={this.props.cancelX}>בטל</button>
        </div>
      </ModalWidow>
    )
  }
}
