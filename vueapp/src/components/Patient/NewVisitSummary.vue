<script setup lang="ts">
// eslint-disable-next-line
const emit = defineEmits(["page", "submit"]);
// eslint-disable-next-line
const props = defineProps({
  data: Object,
});
const change_page = (arg: number) => {
  emit("page", arg);
};

</script>

<template>
  <v-card justify-center>
    <v-card-item>
      <v-container class="d-flex justify-center align-center">
        <v-card
          height="64"
          width="64"
          color="#36BFF1"
          class="d-flex justify-center align-center"
        >
          <v-icon icon="mdi-hospital-building" size="48" color="white"></v-icon>
        </v-card>
      </v-container>
      <v-card-title class="font-weight-bold text-h5" font-size="56"
        >Podsumowanie wizyty</v-card-title
      >
      <v-card-subtitle>Sprawdź i zatwierdź dane</v-card-subtitle>
    </v-card-item>
    <v-spacer></v-spacer>
    <v-card-text>
      <v-form @submit.prevent>
        <v-container>
          <v-row>
            <p class="font-weight-bold">Informacje o wizycie</p>
          </v-row>
          <v-row><v-divider></v-divider></v-row>
          <v-row>
            <v-col
              class="font-weight-bold text-blue-darken-1 text-left"
              cols="4"
            >
              Data wizyty
            </v-col>
            <v-col class="text-left">
              {{ props.data?.date + ", " + props.data?.hour }}
            </v-col>
          </v-row>
          <v-row>
            <v-col
              class="font-weight-bold text-blue-darken-1 text-left"
              cols="4"
            >
              Specjalista
            </v-col>
            <v-col class="text-left"> {{ props.data?.specialization }} </v-col>
          </v-row>
          <v-row>
            <v-col
              cols="4"
              class="font-weight-bold text-blue-darken-1 text-left"
            >
              Podane objawy
            </v-col>
            <v-col class="text-left">
              {{ props.data?.symptoms ? props.data?.symptoms : "Nie podano" }}
            </v-col>
          </v-row>
          <v-row>
            <v-col
              class="font-weight-bold text-blue-darken-1 text-left"
              cols="4"
            >
              Przyjmowane leki
            </v-col>
            <v-col class="text-left">
              {{ props.data?.medicine ? props.data?.medicine : "Nie podano" }}
            </v-col>
          </v-row>
          <v-row><v-divider></v-divider></v-row>
        </v-container>

        <v-row justify="center">
          <v-col xs="12" sm="6" md="3" align-self="center" class="text-left">
            <v-btn
              variant="outlined"
              size="large"
              class="mt-2 button"
              color="blue-darken-2"
              @click="change_page(-1)"
            >
              Wstecz
            </v-btn>
          </v-col>
          <v-col justify="center" class="text-right">
            <v-btn
              xs="12"
              sm="6"
              md="3"
              align-self="center"
              size="large"
              class="mt-2 button"
              color="blue-darken-2"
              @click="emit('submit')"
            >
              Zatwierdź
            </v-btn>
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<style>
.button {
  text-transform: unset !important;
}
</style>
